namespace Mercurius.LAN.Web.DTOs.PublicProfiles;

/// <summary>
/// The bounded public match history and next-match projection for a profile.
/// </summary>
public sealed class PublicProfileMatchSummariesDTO
{
    public IReadOnlyList<PublicProfileMatchSummaryDTO> PreviousMatches { get; init; } = [];
    public IReadOnlyList<PublicProfileMatchSummaryDTO> UpcomingMatches { get; init; } = [];
}
