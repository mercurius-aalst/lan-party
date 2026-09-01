namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class EligibilityResponseDTO
{
    public bool Eligible { get; init; }
    public IReadOnlyList<string> ReasonCodes { get; init; } = [];
}
