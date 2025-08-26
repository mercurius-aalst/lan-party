using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Games
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public GameStatus Status { get; set; }
        public BracketType BracketType { get; set; }
        public string Format { get; set; } = null!;
        public string FinalsFormat { get; set; }= null!;
        public ParticipantType ParticipantType { get; set; }
        public string Description { get; set; } = null!;
        public string RegisterFormUrl { get; set; } = null!;
    }
}