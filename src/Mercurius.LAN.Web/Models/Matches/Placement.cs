using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Matches
{
    public class Placement
    {
        public int Place { get; set; }
        public List<UserDTO> Users { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
    }
}
