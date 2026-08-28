using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class TournamentRosterMemberDTO
{
    public Guid Id { get; init; }
    public PublicUserDTO User { get; init; } = null!;
    public bool IsCaptain { get; init; }
    public RosterMemberConfirmationStatus ConfirmationStatus { get; init; }
}
