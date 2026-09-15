namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class PendingRosterConfirmationPageDTO
{
    public IReadOnlyList<PendingRosterConfirmationDTO> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public sealed class PendingRosterConfirmationDTO
{
    public Guid RosterMemberId { get; init; }
    public Guid TournamentId { get; init; }
    public string TournamentName { get; init; } = string.Empty;
    public Guid TeamId { get; init; }
    public string TeamName { get; init; } = string.Empty;
    public string? TeamLogoUrl { get; init; }
    public DateTime SelectedAtUtc { get; init; }
}
