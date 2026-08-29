using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Tournaments;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentMatchesTab
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<Match> OnDataReload { get; set; }

    private string GetBracketSummary()
    {
        if(!Tournament.Matches.Any())
            return "Bracket progression will appear here once the tournament has been seeded.";

        return $"{Tournament.Matches.Count()} match{(Tournament.Matches.Count() == 1 ? string.Empty : "es")} are currently loaded into the bracket.";
    }
}
