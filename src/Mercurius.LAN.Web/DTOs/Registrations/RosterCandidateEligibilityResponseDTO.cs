namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class RosterCandidateEligibilityResponseDTO
{
    public bool Eligible { get; init; }
    public IReadOnlyList<string> ReasonCodes { get; init; } = [];
    public IReadOnlyList<RosterCandidateEligibilityDTO> Candidates { get; init; } = [];
}
