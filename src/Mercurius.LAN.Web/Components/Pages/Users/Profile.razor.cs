using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;
using System.Net;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class Profile
{
    private readonly UpdateUserProfileRequest _model = new();
    private EditContext? _editContext;
    private string _emailDisplay = string.Empty;
    private bool _emailVerified;
    private string _emailStatusText = string.Empty;
    private string _emailStatusClass = "form-text";
    private string? _usernameAvailabilityMessage;
    private string _usernameAvailabilityClass = "form-text";
    private string _originalUsername = string.Empty;
    private string _deleteConfirmation = string.Empty;
    private bool _isSaving;
    private bool _isSendingVerification;
    private bool _isSendingPasswordReset;
    private bool _isDeleting;
    private bool _canDelete => string.Equals(_deleteConfirmation, "DELETE", StringComparison.Ordinal);

    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var currentProfile = await UserClient.GetCurrentUserProfileAsync();
            if(!currentProfile.IsComplete)
            {
                NavigationManager.NavigateTo("/complete-profile?returnUrl=/profile");
                return;
            }

            ApplyProfile(currentProfile);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Unauthorized)
        {
            NavigationManager.NavigateTo("/account/login?returnUrl=/profile", true);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.Gone)
        {
            NavigationManager.NavigateTo("/account/logout", true);
        }
    }

    private async Task SaveAsync()
    {
        if(_isSaving)
            return;

        _isSaving = true;
        try
        {
            if(!string.Equals(_model.Username.Trim(), _originalUsername, StringComparison.OrdinalIgnoreCase))
            {
                var availability = await UserClient.CheckUsernameAvailabilityAsync(_model.Username);
                if(!availability.IsAvailable)
                {
                    _usernameAvailabilityMessage = availability.Reason ?? "Username is unavailable.";
                    _usernameAvailabilityClass = "form-text text-danger";
                    return;
                }
            }

            var profile = await UserClient.UpdateCurrentUserProfileAsync(_model);
            _originalUsername = profile.Username ?? string.Empty;
            ToastService.ShowSuccess("Profile saved.");
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

        if(string.IsNullOrWhiteSpace(_model.Username) ||
           !System.Text.RegularExpressions.Regex.IsMatch(_model.Username.Trim(), "^[a-zA-Z0-9]{3,32}$") ||
           string.Equals(_model.Username.Trim(), _originalUsername, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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

    private async Task ResendVerificationEmailAsync()
    {
        _isSendingVerification = true;
        try
        {
            var result = await UserClient.ResendVerificationEmailAsync();
            ToastService.ShowInfo(result.Message);
        }
        catch(ApiException exception)
        {
            ToastService.ShowError(await GetApiErrorAsync(exception));
        }
        finally
        {
            _isSendingVerification = false;
        }
    }

    private async Task SendPasswordResetEmailAsync()
    {
        _isSendingPasswordReset = true;
        try
        {
            var result = await UserClient.SendPasswordResetEmailAsync();
            ToastService.ShowInfo(result.Message);
        }
        catch(ApiException exception)
        {
            ToastService.ShowError(await GetApiErrorAsync(exception));
        }
        finally
        {
            _isSendingPasswordReset = false;
        }
    }

    private async Task DeleteAccountAsync()
    {
        if(!_canDelete || _isDeleting)
            return;

        _isDeleting = true;
        try
        {
            await UserClient.DeleteCurrentUserAsync();
            NavigationManager.NavigateTo("/account/logout", true);
        }
        catch(ApiException exception)
        {
            ToastService.ShowError(await GetApiErrorAsync(exception));
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private void ApplyProfile(CurrentUserProfileResponse response)
    {
        var profile = response.User;
        _model.Username = profile?.Username ?? string.Empty;
        _model.Firstname = profile?.Firstname ?? string.Empty;
        _model.Lastname = profile?.Lastname ?? string.Empty;
        _model.DiscordId = profile?.DiscordId;
        _model.SteamId = profile?.SteamId;
        _model.RiotId = profile?.RiotId;
        _originalUsername = _model.Username;

        _emailDisplay = response.Email ?? profile?.Email ?? string.Empty;
        _emailVerified = response.EmailVerified || profile?.EmailVerified == true;
        _emailStatusText = _emailVerified ? "Verified" : "Unverified";
        _emailStatusClass = _emailVerified ? "form-text text-success" : "form-text text-warning";

        _editContext = new EditContext(_model);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
    }

    private static async Task<string> GetApiErrorAsync(ApiException exception)
    {
        var content = await exception.GetContentAsAsync<string>();
        return string.IsNullOrWhiteSpace(content) ? "Request failed." : content;
    }
}
