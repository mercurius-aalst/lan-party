namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class SubmitTeamRosterDTO
{
    public Guid TeamId { get; init; }
    public IReadOnlyList<Guid> UserIds { get; init; } = [];
}
