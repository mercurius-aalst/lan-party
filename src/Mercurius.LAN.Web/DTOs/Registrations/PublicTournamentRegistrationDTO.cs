using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public class PublicTournamentRegistrationDTO
{
    public Guid Id { get; init; }
    public Guid TournamentId { get; init; }
    public TournamentRegistrationKind Kind { get; init; }
    public TournamentRegistrationStatus Status { get; init; }
    public PublicUserDTO? User { get; init; }
    public PublicTournamentTeamDTO? Team { get; init; }
    public IReadOnlyList<PublicTournamentRosterMemberDTO> RosterMembers { get; init; } = [];
}
