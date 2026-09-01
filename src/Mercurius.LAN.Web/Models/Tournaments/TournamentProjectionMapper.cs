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
    public static void ApplyRegistration(TournamentExtended tournament, TournamentRegistrationDTO registration)
    {
        var registrations = tournament.Registrations?.ToList() ?? [];
        registrations.RemoveAll(existing =>
            existing.Id == registration.Id ||
            registration.Team is not null &&
            registration.Kind == TournamentRegistrationKind.Team &&
            existing.Kind == TournamentRegistrationKind.Team &&
            existing.Team?.Id == registration.Team.Id);

        if(registration.Status == TournamentRegistrationStatus.Active)
            registrations.Add(ToPublicRegistration(registration));

        tournament.Registrations = registrations;
        PopulateParticipantProjection(tournament);
    }

    public static void RemoveRegistration(TournamentExtended tournament, Guid registrationId)
    {
        tournament.Registrations = (tournament.Registrations ?? [])
            .Where(registration => registration.Id != registrationId)
            .ToList();
        PopulateParticipantProjection(tournament);
    }

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

    private static PublicTournamentRegistrationDTO ToPublicRegistration(TournamentRegistrationDTO registration)
    {
        return new PublicTournamentRegistrationDTO
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,
            Kind = registration.Kind,
            Status = registration.Status,
            User = registration.User,
            Team = registration.Team is null
                ? null
                : new PublicTournamentTeamDTO
                {
                    Id = registration.Team.Id,
                    Name = registration.Team.Name,
                    CaptainUserId = registration.Team.CaptainUserId,
                    LogoUrl = registration.Team.LogoUrl
                },
            RosterMembers = registration.RosterMembers
                .Select(member => new PublicTournamentRosterMemberDTO
                {
                    User = member.User,
                    IsCaptain = member.IsCaptain
                })
                .ToList()
        };
    }
}
