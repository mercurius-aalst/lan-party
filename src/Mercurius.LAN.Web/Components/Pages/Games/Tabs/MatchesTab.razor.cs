using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class MatchesTab
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback OnDataReload { get; set; }
}
