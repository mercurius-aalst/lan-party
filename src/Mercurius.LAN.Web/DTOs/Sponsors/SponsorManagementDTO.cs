using Mercurius.LAN.Web.Models.Sponsors;
using Microsoft.AspNetCore.Components.Forms;

namespace Mercurius.LAN.Web.DTOs.Sponsors
{
    public class SponsorManagementDTO
    {
        public string Name { get; set; }
        public int SponsorTier { get; set; }
        public IBrowserFile? Logo { get; set; }
        public string InfoUrl { get; set; }
        public int Id { get; set; } = -1;

        public SponsorManagementDTO(Sponsor sponsor)
        {
            Name = sponsor.Name;
            SponsorTier = sponsor.SponsorTier;
            InfoUrl = sponsor.InfoUrl;
            Id = sponsor.Id;
        }
        public SponsorManagementDTO()
        {
            
        }
    }
}
