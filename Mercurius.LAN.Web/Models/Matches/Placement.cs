using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Matches
{
    public class Placement
    {
        public int Place { get; set; }
        public List<Participant> Participants { get; set; } = new();
    }
}