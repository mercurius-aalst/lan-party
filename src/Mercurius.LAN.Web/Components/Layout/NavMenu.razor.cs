using Microsoft.AspNetCore.Components;
using Mercurius.LAN.Web.Services;

namespace Mercurius.LAN.Web.Components.Layout;

public partial class NavMenu
{
    [Inject]
    private IAuthService AuthService { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    private bool _isUserMenuVisible = false;
    private bool _isDropdownVisible = false;

    private void Logout()
    {
        NavigationManager.NavigateTo("/logout", true);
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
}