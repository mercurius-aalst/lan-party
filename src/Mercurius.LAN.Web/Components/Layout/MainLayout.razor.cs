// Moved @code block from MainLayout.razor
namespace Mercurius.LAN.Web.Components.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = false;


    private void ToggleSidebar() => _sidebarOpen = !_sidebarOpen;
 
}