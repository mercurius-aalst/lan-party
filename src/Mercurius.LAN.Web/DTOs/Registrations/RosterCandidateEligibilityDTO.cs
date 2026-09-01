using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class RosterCandidateEligibilityDTO
{
    public Guid UserId { get; init; }
    public PublicUserDTO? User { get; init; }
    public bool Eligible { get; init; }
    public IReadOnlyList<string> ReasonCodes { get; init; } = [];
}
