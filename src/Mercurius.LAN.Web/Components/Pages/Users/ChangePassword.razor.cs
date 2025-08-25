using Microsoft.AspNetCore.Components;
using Mercurius.LAN.Web.Services;
using Refit;
using Blazored.Toast.Services;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class ChangePassword
{
    [Inject] 
    private IAuthService AuthService { get; set; }
    [Inject] 
    private IUserService UserService { get; set; }
    [Inject] 
    private NavigationManager NavigationManager { get; set; }
    [Inject]
    private IToastService ToastService { get; set; }
    private ChangePasswordModel _changePasswordModel = new();
    private string? _errorMessage;

    private async Task HandleChangePasswordAsync()
    {
        if (_changePasswordModel.NewPassword != _changePasswordModel.ConfirmPassword)
        {
            ToastService.ShowError("New password and confirmation do not match.");
            return;
        }

        try
        {
            await UserService.ChangePasswordAsync(AuthService.CurrentUser.Identity.Name, _changePasswordModel.CurrentPassword, _changePasswordModel.NewPassword);
            ToastService.ShowSuccess("Password changed successfully.");
            NavigationManager.NavigateTo("/", true);
        }
        catch (ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }

    private class ChangePasswordModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}