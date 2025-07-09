using System.Collections.Generic;

namespace MercuriusAalstLanParty.Client.Models
{
    public class Placement
    {
        public int Place { get; set; }
        public IEnumerable<Participant> Participants { get; set; } = new List<Participant>();
    }
}
