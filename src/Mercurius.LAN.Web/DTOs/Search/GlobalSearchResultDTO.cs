namespace Mercurius.LAN.Web.DTOs.Search;

public class GlobalSearchResultDTO
{
    public GlobalSearchResultType Type { get; set; }
    public string DisplayLabel { get; set; } = string.Empty;
    public string? SupportingText { get; set; }
    public string? Username { get; set; }
    public string? TeamName { get; set; }
    public Guid? GameId { get; set; }
}
