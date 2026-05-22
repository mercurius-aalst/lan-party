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
    private bool _isInfoMenuVisible = false;

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

    private async Task Logout()
    {
        CloseDropdown();
        await OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo("/account/logout", true);
    }

    private void ToggleUserMenu() => _isUserMenuVisible = !_isUserMenuVisible;

    private void HandleOutsideClick()
    {
        _isUserMenuVisible = false;
        _isDropdownVisible = false;
        _isInfoMenuVisible = false;
    }

    private void ToggleDropdown()
    {
        _isDropdownVisible = !_isDropdownVisible;
        _isUserMenuVisible = false;
        _isInfoMenuVisible = false;
    }

    private void ToggleInfoMenu()
    {
        _isInfoMenuVisible = !_isInfoMenuVisible;
        _isDropdownVisible = false;
        _isUserMenuVisible = false;
    }

    private void CloseDropdown()
    {
        _isDropdownVisible = false;
        _isUserMenuVisible = false;
        _isInfoMenuVisible = false;
    }

    private async Task HandleNavigationClicked()
    {
        CloseDropdown();
        await OnNavigationSelected.InvokeAsync();
    }

    private string GetAdminButtonClass() => GetUtilityButtonClass(_isDropdownVisible);

    private string GetInfoButtonClass()
    {
        var classes = "brand-nav-link info-nav-button text-center md:text-left";
        return _isInfoMenuVisible || IsInfoPage ? $"{classes} brand-nav-link--active" : classes;
    }

    private string GetUserButtonClass() => $"{GetUtilityButtonClass(_isUserMenuVisible)} user-button";

    private bool IsInfoPage => NavigationManager.ToBaseRelativePath(NavigationManager.Uri).StartsWith("info", StringComparison.OrdinalIgnoreCase);

    private static string GetAriaExpanded(bool isExpanded) => isExpanded ? "true" : "false";

    private static string GetUtilityButtonClass(bool isOpen)
    {
        var classes = "brand-utility-button w-full justify-center md:w-auto";
        return isOpen ? $"{classes} brand-utility-button--open" : classes;
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
