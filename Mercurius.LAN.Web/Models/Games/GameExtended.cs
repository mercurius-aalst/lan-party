using System.Collections.Generic;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Games
{
    public class GameExtended : Game
    {
        public IEnumerable<Placement> Placements { get; set; } = new List<Placement>();
        public IEnumerable<Match> Matches { get; set; } = new List<Match>();
        public IEnumerable<Participant> Participants { get; set; } = new List<Participant>();
    }
}