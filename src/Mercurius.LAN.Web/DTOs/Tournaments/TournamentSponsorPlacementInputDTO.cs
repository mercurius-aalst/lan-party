using Mercurius.LAN.Web.Models.Sponsors;

namespace Mercurius.LAN.Web.DTOs.Tournaments;

public class TournamentSponsorPlacementInputDTO
{
    public int SponsorId { get; set; }
    public SponsorContext Context { get; set; }
    public string? Headline { get; set; }
    public string? SupportLine { get; set; }
    public int DisplayOrder { get; set; }
}
