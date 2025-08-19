using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class LoadingScreen
{
    [Parameter] public string? Message { get; set; }
}