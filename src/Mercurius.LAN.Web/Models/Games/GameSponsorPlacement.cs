using Mercurius.LAN.Web.Models.Sponsors;

namespace Mercurius.LAN.Web.Models.Games;

public class GameSponsorPlacement
{
    public int Id { get; set; }
    public int SponsorId { get; set; }
    public string SponsorName { get; set; } = null!;
    public SponsorTier SponsorTier { get; set; }
    public string SponsorLogoUrl { get; set; } = null!;
    public string SponsorInfoUrl { get; set; } = null!;
    public string? SponsorDescription { get; set; }
    public SponsorContext Context { get; set; }
    public string? Headline { get; set; }
    public string? SupportLine { get; set; }
    public int DisplayOrder { get; set; }
}
