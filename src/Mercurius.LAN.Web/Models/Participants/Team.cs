using Mercurius.LAN.Web.DTOs.Users;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Models.Participants
{
    public class Team
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public Guid CaptainUserId { get; set; }
        public string? LogoUrl { get; set; }
        public IEnumerable<PublicUserDTO> Members { get; set; } = new List<PublicUserDTO>();
        public IEnumerable<TeamInvite> TeamInvites { get; set; } = new List<TeamInvite>();
    }
}
