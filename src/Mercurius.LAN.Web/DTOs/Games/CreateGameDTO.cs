using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class CreateGameDTO
    {
        [Required]
        public string Name { get; set; }
      
        [Required]
        public BracketType BracketType { get; set; }
  
        [Required]
        public GameFormat Format { get; set; }
   
        [Required]
        public GameFormat FinalsFormat { get; set; }
  
        [Required]
        public ParticipantType ParticipantType { get; set; }
    
        [Required]
        public IBrowserFile Image { get; set; }

        [Required]
        public string RegisterFormUrl { get; set; }
    }
}
