namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed record TournamentRosterConfirmationChangedEvent(
    Guid TeamId,
    Guid RosterMemberId,
    Guid UserId,
    string Status);
