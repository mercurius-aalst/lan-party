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
        var activeRegistrations = registrations
            .Where(registration => registration.Status == TournamentRegistrationStatus.Active)
            .ToList();

        tournament.Users = activeRegistrations
            .Where(registration => registration.Kind == TournamentRegistrationKind.Individual)
            .Select(registration => registration.User)
            .Where(user => user is not null)
            .Select(user => user!)
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .ToList();

        tournament.Teams = activeRegistrations
            .Where(registration => registration.Kind == TournamentRegistrationKind.Team)
            .Where(registration => registration.Team is not null)
            .Select(registration => ToTeam(registration.Team!, registration.RosterMembers))
            .GroupBy(team => team.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static Team ToTeam(
        PublicTournamentTeamDTO team,
        IEnumerable<PublicTournamentRosterMemberDTO> rosterMembers)
    {
        return new Team
        {
            Id = team.Id,
            Name = team.Name,
            CaptainUserId = team.CaptainUserId,
            LogoUrl = team.LogoUrl,
            Members = rosterMembers
                .Select(rosterMember => rosterMember.User)
                .Where(user => user is not null)
                .GroupBy(user => user!.Id)
                .Select(group => group.First()!)
                .ToList()
        };
    }
}
