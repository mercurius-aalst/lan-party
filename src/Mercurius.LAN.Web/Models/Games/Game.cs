using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Games
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public GameStatus Status { get; set; }
        public BracketType BracketType { get; set; }
        public string Format { get; set; }
        public string FinalsFormat { get; set; }
        public ParticipantType ParticipantType { get; set; }
        public string Description { get; set; }
    }
}