using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class PublicTournamentRosterMemberDTO
{
    public PublicUserDTO User { get; init; } = null!;
    public bool IsCaptain { get; init; }
}
