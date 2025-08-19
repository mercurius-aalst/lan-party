// Moved @code block from MatchesTab.razor
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class MatchesTab
{
    [Parameter] public IEnumerable<Match> Matches { get; set; }
    [Parameter] public IEnumerable<Participant> Participants { get; set; }
    [Parameter] public BracketType BracketType { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
}