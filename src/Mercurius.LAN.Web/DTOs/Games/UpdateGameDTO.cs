using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class UpdateGameDTO
    {
        public string Name { get; set; }
        public GameFormat Format { get; set; }
        public GameFormat FinalsFormat { get; set; }
        public BracketType BracketType { get; set; }
        
    }
}
