using Mercurius.LAN.Web.Models.Sponsors;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Sponsors
{
    public class SponsorManagementDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [Range(1, 5)]
        public int SponsorTier { get; set; }


        [RequiredIfCreateMode(ErrorMessageResourceName = nameof(Logo))]
        public IBrowserFile? Logo { get; set; } = null!;

        public bool IsCreateMode { get; set; } = true;

        [Required]
        public string InfoUrl { get; set; } = null!;

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
