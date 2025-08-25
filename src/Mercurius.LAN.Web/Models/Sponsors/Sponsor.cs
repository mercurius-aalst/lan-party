namespace Mercurius.LAN.Web.Models.Sponsors
{
    public class Sponsor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SponsorTier { get; set; }
        public string LogoUrl { get; set; }
        public string InfoUrl { get; set; }
    }
}
