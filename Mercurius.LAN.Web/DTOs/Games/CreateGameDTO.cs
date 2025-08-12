using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class CreateGameDTO
    {
        public string Name { get; set; }
        public BracketType BracketType { get; set; }
        public GameFormat Format { get; set; }
        public GameFormat FinalsFormat { get; set; }
        public ParticipantType ParticipantType { get; set; }
    }
}
