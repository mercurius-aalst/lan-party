using Mercurius.LAN.Web.Models.Sponsors;

namespace Mercurius.LAN.Web.Extensions;

public static class SponsorExtensions
{
    public static string GetLabel(this SponsorTier sponsorTier)
    {
        return sponsorTier switch
        {
            SponsorTier.Presenting => "Presenting Partners",
            SponsorTier.Gold => "Gold Partners",
            SponsorTier.Silver => "Silver Partners",
            SponsorTier.Bronze => "Bronze Partners",
            _ => sponsorTier.ToString()
        };
    }

    public static string GetShortLabel(this SponsorTier sponsorTier)
    {
        return sponsorTier switch
        {
            SponsorTier.Presenting => "Presenting",
            SponsorTier.Gold => "Gold",
            SponsorTier.Silver => "Silver",
            SponsorTier.Bronze => "Bronze",
            _ => sponsorTier.ToString()
        };
    }

    public static int GetDisplayOrder(this SponsorTier sponsorTier)
    {
        return sponsorTier switch
        {
            SponsorTier.Presenting => 0,
            SponsorTier.Gold => 1,
            SponsorTier.Silver => 2,
            SponsorTier.Bronze => 3,
            _ => int.MaxValue
        };
    }

    public static string GetLabel(this SponsorContext sponsorContext)
    {
        return sponsorContext switch
        {
            SponsorContext.TournamentPartner => "Tournament Partner",
            SponsorContext.CateringPartner => "Catering Partner",
            SponsorContext.InfrastructurePartner => "Infrastructure Partner",
            SponsorContext.PrizePartner => "Prize Partner",
            _ => sponsorContext.ToString()
        };
    }

    public static string GetSectionEyebrow(this SponsorContext sponsorContext)
    {
        return sponsorContext switch
        {
            SponsorContext.TournamentPartner => "Presented by",
            SponsorContext.CateringPartner => "Food & Drinks",
            SponsorContext.InfrastructurePartner => "Connectivity",
            SponsorContext.PrizePartner => "Prizes",
            _ => sponsorContext.ToString()
        };
    }

    public static int GetDisplayOrder(this SponsorContext sponsorContext)
    {
        return sponsorContext switch
        {
            SponsorContext.TournamentPartner => 0,
            SponsorContext.PrizePartner => 1,
            SponsorContext.InfrastructurePartner => 2,
            SponsorContext.CateringPartner => 3,
            _ => int.MaxValue
        };
    }
}
