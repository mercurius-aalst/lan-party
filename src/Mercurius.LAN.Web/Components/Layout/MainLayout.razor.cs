using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Mercurius.LAN.Web.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private bool _sidebarOpen = false;

    private bool IsHomePage
    {
        get
        {
            var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
            var routeOnly = relativePath.Split('?', '#')[0].TrimEnd('/');
            return string.IsNullOrEmpty(routeOnly);
        }
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

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

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if(!_sidebarOpen)
            return;

        _sidebarOpen = false;
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
