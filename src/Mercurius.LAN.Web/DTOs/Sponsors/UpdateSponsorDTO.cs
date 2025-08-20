namespace Mercurius.LAN.Web.DTOs.Sponsors
{
    public class UpdateSponsorDTO
    {
        public string Name { get; set; }
        public int SponsorTier { get; set; }
        public IFormFile Logo { get; set; }
        public string InfoUrl { get; set; }
    }
}
