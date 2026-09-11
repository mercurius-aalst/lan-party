using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Tournaments;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentMatchesTab
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<Match> OnDataReload { get; set; }
    [Parameter] public EventCallback<Match> OnMatchRefreshed { get; set; }
}
