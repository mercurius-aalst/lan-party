using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public class TournamentRegistrationDTO
{
    public Guid Id { get; init; }
    public Guid TournamentId { get; init; }
    public TournamentRegistrationKind Kind { get; init; }
    public TournamentRegistrationStatus Status { get; init; }
    public PublicUserDTO? User { get; init; }
    public TeamParticipantDTO? Team { get; init; }
    public IReadOnlyList<TournamentRosterMemberDTO> RosterMembers { get; init; } = [];
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
