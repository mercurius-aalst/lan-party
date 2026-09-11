using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentPlacementsTab
{
    [Parameter] public IEnumerable<Placement> Placements { get; set; } = Enumerable.Empty<Placement>();
    [Parameter] public ParticipationMode ParticipationMode { get; set; }

    private IEnumerable<string> GetUserParticipantNames(Placement placement)
    {
        return ParticipationMode == ParticipationMode.Individual
            ? placement.Users.Select(GetUserLabel)
            : Enumerable.Empty<string>();
    }

    private static string BuildTeamProfileHref(string teamName) =>
        string.IsNullOrWhiteSpace(teamName)
            ? string.Empty
            : $"/teams/{Uri.EscapeDataString(teamName.Trim())}";

    private string GetUserLabel(DTOs.Users.PublicUserDTO user)
    {
        if(!string.IsNullOrWhiteSpace(user.Username))
            return user.Username.Trim();

        return string.IsNullOrWhiteSpace(user.DisplayName)
            ? Localization["Feature.tournaments.participant"]
            : user.DisplayName.Trim();
    }

    private string GetOrdinalLabel(int number)
    {
        return Localization.Get("Feature.tournaments.placementOrdinal", number);
    }
}
