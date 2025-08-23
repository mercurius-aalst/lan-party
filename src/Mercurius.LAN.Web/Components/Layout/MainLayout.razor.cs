// Moved @code block from MainLayout.razor
namespace Mercurius.LAN.Web.Components.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = false;


    private void ToggleSidebar() => _sidebarOpen = !_sidebarOpen;
 
    private void HandleOutsideClick()
    {
        _sidebarOpen = false;
    }

    private void OnNavigationSelected()
    {
        if(_sidebarOpen)
            _sidebarOpen = !_sidebarOpen;
    }
}