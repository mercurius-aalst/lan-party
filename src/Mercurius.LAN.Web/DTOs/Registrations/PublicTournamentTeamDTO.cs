using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class PublicTournamentTeamDTO
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CaptainUserId { get; init; }
    public string? LogoUrl { get; set; }

    // The public backend projection does not currently populate members. Keep this empty-safe
    // compatibility property for the existing participant projection; authenticated registration
    // responses use TeamParticipantDTO when roster members are part of the contract.
    public IReadOnlyList<PublicUserDTO> Members { get; init; } = [];
}
