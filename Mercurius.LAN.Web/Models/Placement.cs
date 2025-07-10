using System.Collections.Generic;

namespace Mercurius.LAN.Web.Models
{
    public class Placement
    {
        public int Place { get; set; }
        public List<Participant> Participants { get; set; } = new();
    }
}
