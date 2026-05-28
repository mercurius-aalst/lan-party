using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class MatchesTab
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback OnDataReload { get; set; }

    private string GetBracketSummary()
    {
        if(!Game.Matches.Any())
            return "Bracket progression will appear here once the tournament has been seeded.";

        return $"{Game.Matches.Count()} match{(Game.Matches.Count() == 1 ? string.Empty : "es")} are currently loaded into the bracket.";
    }
}
