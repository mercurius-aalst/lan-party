using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components.Forms;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class CreateGameDTO
    {
        public string Name { get; set; }
        public BracketType BracketType { get; set; }
        public GameFormat Format { get; set; }
        public GameFormat FinalsFormat { get; set; }
        public ParticipantType ParticipantType { get; set; }
        public IBrowserFile? Image { get; set; }
        public string RegistrationUrl { get; set; }
    }
}
