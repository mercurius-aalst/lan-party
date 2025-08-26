using Mercurius.LAN.Web.Models.Participants;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Participants.Players
{
    public class UpdateAndCreatePlayerFormDTO
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Firstname { get; set; } = null!;
        [Required]
        public string Lastname { get; set; } = null!;
        public string? DiscordId { get; set; }
        public string? SteamId { get; set; }
        public string? RiotId { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        public int? Id { get; set; }

        public UpdateAndCreatePlayerFormDTO()
        {
            
        }

        public UpdateAndCreatePlayerFormDTO(Player player)
        {
            Username = player.Username;
            Firstname = player.Firstname;
            Lastname = player.Lastname;
            DiscordId = player.DiscordId;
            SteamId = player.SteamId;
            RiotId = player.RiotId;
            Email = player.Email;
            Id = player.Id;
        }
    }
}
