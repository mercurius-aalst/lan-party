using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

/// <summary>
/// Team projection returned inside an authenticated tournament registration.
/// This intentionally remains separate from <see cref="PublicTournamentTeamDTO"/>,
/// whose public registration projection does not expose roster members.
/// </summary>
public sealed class TeamParticipantDTO
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CaptainUserId { get; init; }
    public string? LogoUrl { get; init; }
    public IReadOnlyList<PublicUserDTO> Members { get; init; } = [];
}
