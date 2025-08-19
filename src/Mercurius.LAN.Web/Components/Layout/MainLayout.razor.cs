// Moved @code block from MainLayout.razor
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Layout;

public partial class MainLayout
{
    private bool _sidebarOpen = false;

    private void ToggleSidebar() => _sidebarOpen = !_sidebarOpen;
}