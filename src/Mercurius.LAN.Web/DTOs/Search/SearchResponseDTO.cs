namespace Mercurius.LAN.Web.DTOs.Search;

public sealed class SearchResponseDTO
{
    public IReadOnlyList<GlobalSearchResultDTO> Results { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}
