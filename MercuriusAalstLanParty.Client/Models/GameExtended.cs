using System.Collections.Generic;

namespace MercuriusAalstLanParty.Client.Models
{
    public class GameExtended : Game
    {
        public IEnumerable<Placement> Placements { get; set; } = new List<Placement>();
        public IEnumerable<Match> Matches { get; set; } = new List<Match>();
        public IEnumerable<Participant> Participants { get; set; } = new List<Participant>();
    }
}
