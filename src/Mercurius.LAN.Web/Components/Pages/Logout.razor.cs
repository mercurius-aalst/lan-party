using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Refit;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Services;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Logout
{
    [CascadingParameter] private HttpContext? HttpContext { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await AuthService.LogoutAsync();

            // Remove cookies
            HttpContext?.Response.Cookies.Delete("access_token");
            HttpContext?.Response.Cookies.Delete("refresh_token");

            await InvokeAsync(StateHasChanged);
            ToastService.ShowInfo("You have successfully logged out.");

            NavigationManager.NavigateTo("/login", true);
        }
        catch (ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }
}