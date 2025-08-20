using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Sponsors
{
    public class CreateSponsorDTO
    {
        public string Name { get; set; }
        public int SponsorTier { get; set; }
        public IFormFile Logo { get; set; }
        public string InfoUrl { get; set; }
    }
}
