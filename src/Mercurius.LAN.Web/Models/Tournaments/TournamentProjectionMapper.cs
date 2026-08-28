using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Tournaments;

/// <summary>
/// Projects the canonical registration collection into the participant collections used by
/// existing bracket and identity display components. The API detail response remains the source
/// of truth; these collections are compatibility projections only.
/// </summary>
public static class TournamentProjectionMapper
{
    public static void PopulateParticipantProjection(TournamentExtended tournament)
    {
        var registrations = tournament.Registrations?.ToList() ?? [];
        if(registrations.Count == 0)
            return;

        tournament.Users = registrations
            .Where(registration => registration.Kind == TournamentRegistrationKind.Individual)
            .Where(registration => registration.Status == TournamentRegistrationStatus.Active)
            .Select(registration => registration.User)
            .Where(user => user is not null)
            .Select(user => user!)
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .ToList();

        tournament.Teams = registrations
            .Where(registration => registration.Kind == TournamentRegistrationKind.Team)
            .Where(registration => registration.Status == TournamentRegistrationStatus.Active)
            .Select(registration => registration.Team)
            .Where(team => team is not null)
            .Select(team => ToTeam(team!))
            .GroupBy(team => team.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static Team ToTeam(PublicTournamentTeamDTO team)
    {
        return new Team
        {
            Id = team.Id,
            Name = team.Name,
            CaptainUserId = team.CaptainUserId,
            LogoUrl = team.LogoUrl,
            Members = team.Members
        };
    }
}
