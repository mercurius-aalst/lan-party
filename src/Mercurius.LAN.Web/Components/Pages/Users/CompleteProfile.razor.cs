using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Users;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Refit;
using System.Net;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class CompleteProfile
{
    private readonly CompleteUserProfileRequest _model = new();

    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        PrefillFromClaims(authenticationState.User);

        try
        {
            var currentProfile = await UserClient.GetCurrentUserProfileAsync();
            if(currentProfile.HasProfile)
            {
                NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
            }
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = GetSafeReturnUrl(ReturnUrl);
            NavigationManager.NavigateTo($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
        }
        catch(UnauthorizedAccessException)
        {
            var returnUrl = GetSafeReturnUrl(ReturnUrl);
            NavigationManager.NavigateTo($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
        }
    }

    private async Task SaveAsync()
    {
        await UserClient.CompleteCurrentUserProfileAsync(_model);
        ToastService.ShowSuccess("Profile completed.");
        NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
    }

    private void PrefillFromClaims(ClaimsPrincipal user)
    {
        _model.Email = FindClaim(user, ClaimTypes.Email) ?? _model.Email;
        _model.Username = FindClaim(user, "preferred_username", "nickname")
            ?? GetUsernameFromEmail(_model.Email)
            ?? _model.Username;

        _model.Firstname = FindClaim(user, "given_name") ?? _model.Firstname;
        _model.Lastname = FindClaim(user, "family_name") ?? _model.Lastname;

        if(string.IsNullOrWhiteSpace(_model.Firstname) && string.IsNullOrWhiteSpace(_model.Lastname))
        {
            var name = FindClaim(user, "name");
            if(!string.IsNullOrWhiteSpace(name))
            {
                var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _model.Firstname = nameParts.ElementAtOrDefault(0) ?? _model.Firstname;
                _model.Lastname = nameParts.ElementAtOrDefault(1) ?? _model.Lastname;
            }
        }
    }

    private static string? FindClaim(ClaimsPrincipal user, params string[] claimTypes)
    {
        foreach(var claimType in claimTypes)
        {
            var claimValue = user.FindFirst(claimType)?.Value;
            if(!string.IsNullOrWhiteSpace(claimValue))
                return claimValue;
        }

        return null;
    }

    private static string? GetUsernameFromEmail(string email)
    {
        if(string.IsNullOrWhiteSpace(email))
            return null;

        var atIndex = email.IndexOf('@', StringComparison.Ordinal);
        return atIndex > 0 ? email[..atIndex] : null;
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if(string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        if(!Uri.TryCreate(returnUrl, UriKind.Relative, out _))
            return "/";

        if(!returnUrl.StartsWith("/", StringComparison.Ordinal) ||
           returnUrl.StartsWith("//", StringComparison.Ordinal) ||
           returnUrl.StartsWith("/\\", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }
}
