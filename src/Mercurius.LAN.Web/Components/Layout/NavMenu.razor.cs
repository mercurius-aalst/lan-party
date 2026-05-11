using Microsoft.AspNetCore.Components;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Components.Layout;

public partial class NavMenu
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;
    private bool _isUserMenuVisible = false;
    private bool _isDropdownVisible = false;
    [Parameter]
    public EventCallback OnNavigationSelected { get; set; }
    private string LoginHref => $"/account/login?returnUrl={Uri.EscapeDataString(GetCurrentRelativeUrl())}";
    private string MockAdminLoginHref => $"/account/login?persona=admin&returnUrl={Uri.EscapeDataString("/admin/teams")}";
    private bool IsMockBackendEnabled => Configuration.GetValue<bool>("MockBackend:Enabled");

    private async Task BeginLogin()
    {
        CloseDropdown();
        await OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo(LoginHref, forceLoad: true);
    }

    private void Logout()
    {
        _isUserMenuVisible = false;
        OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo("/account/logout", true);
    }

    private void ToggleUserMenu() => _isUserMenuVisible = !_isUserMenuVisible;

    private void HandleOutsideClick()
    {
        _isUserMenuVisible = false;
        _isDropdownVisible = false;
    }

    private void ToggleDropdown()
    {
        _isDropdownVisible = !_isDropdownVisible;
    }

    private void CloseDropdown()
    {
        _isDropdownVisible = false;
        _isUserMenuVisible = false;
    }

    private async Task HandleNavigationClicked()
    {
        CloseDropdown();
        await OnNavigationSelected.InvokeAsync();
    }

    private string GetCurrentRelativeUrl()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        return string.IsNullOrWhiteSpace(relativePath)
            ? "/"
            : $"/{relativePath}";
    }

    private static string GetDisplayName(ClaimsPrincipal user)
    {
        return user.Identity?.Name
            ?? user.FindFirst("name")?.Value
            ?? user.FindFirst("email")?.Value
            ?? "Account";
    }
}
