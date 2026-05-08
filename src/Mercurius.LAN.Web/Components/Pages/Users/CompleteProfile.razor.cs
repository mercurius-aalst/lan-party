using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Refit;
using System.Net;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class CompleteProfile
{
    private readonly CompleteUserProfileRequest _model = new();
    private EditContext? _editContext;
    private string? _email;
    private string? _usernameAvailabilityMessage;
    private string _usernameAvailabilityClass = "form-text";
    private bool _isSaving;

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
            _email = currentProfile.Email ?? currentProfile.User?.Email ?? _email;
            if(currentProfile.IsComplete)
            {
                NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
                return;
            }
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = GetSafeReturnUrl(ReturnUrl);
            NavigationManager.NavigateTo($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
            return;
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Gone)
        {
            NavigationManager.NavigateTo("/account/logout", true);
            return;
        }
        catch(UnauthorizedAccessException)
        {
            var returnUrl = GetSafeReturnUrl(ReturnUrl);
            NavigationManager.NavigateTo($"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
            return;
        }

        _editContext = new EditContext(_model);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
    }

    private async Task SaveAsync()
    {
        if(_isSaving)
            return;

        _isSaving = true;
        try
        {
            var availability = await UserClient.CheckUsernameAvailabilityAsync(_model.Username);
            if(!availability.IsAvailable)
            {
                _usernameAvailabilityMessage = availability.Reason ?? "Username is unavailable.";
                _usernameAvailabilityClass = "form-text text-danger";
                return;
            }

            await UserClient.CompleteCurrentUserProfileAsync(_model);
            ToastService.ShowSuccess("Profile completed.");
            NavigationManager.NavigateTo(GetSafeReturnUrl(ReturnUrl), true);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.BadRequest || exception.StatusCode == HttpStatusCode.Conflict)
        {
            ToastService.ShowError(await GetApiErrorAsync(exception));
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task CheckUsernameAvailabilityAsync()
    {
        _usernameAvailabilityMessage = null;

        if(string.IsNullOrWhiteSpace(_model.Username) || !System.Text.RegularExpressions.Regex.IsMatch(_model.Username.Trim(), "^[a-zA-Z0-9]{3,32}$"))
            return;

        try
        {
            var availability = await UserClient.CheckUsernameAvailabilityAsync(_model.Username);
            _usernameAvailabilityMessage = availability.IsAvailable
                ? "Username is available."
                : availability.Reason ?? "Username is unavailable.";
            _usernameAvailabilityClass = availability.IsAvailable ? "form-text text-success" : "form-text text-danger";
        }
        catch(ApiException)
        {
            _usernameAvailabilityMessage = null;
        }
    }

    private void PrefillFromClaims(ClaimsPrincipal user)
    {
        _email = FindClaim(user, ClaimTypes.Email);
        _model.Username = FindClaim(user, "preferred_username", "nickname")
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

    private static async Task<string> GetApiErrorAsync(ApiException exception)
    {
        var content = await exception.GetContentAsAsync<string>();
        return string.IsNullOrWhiteSpace(content) ? "Profile could not be saved." : content;
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
