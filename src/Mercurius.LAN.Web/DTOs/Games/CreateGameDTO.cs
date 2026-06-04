using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class CreateGameDTO
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public BracketType BracketType { get; set; }

        [Required]
        public GameFormat Format { get; set; }

        [Required]
        public GameFormat FinalsFormat { get; set; }

        [Required]
        public ParticipationMode ParticipationMode { get; set; }

        [Required]
        public IBrowserFile Image { get; set; } = null!;

        [Required]
        public string RegisterFormUrl { get; set; } = null!;

        [Required]
        public DateTime PlannedStartTime { get; set; } = DateTime.UtcNow.AddDays(7);

        [Range(1, 1440)]
        public int AverageGameDurationMinutes { get; set; } = 60;

        [Range(1, int.MaxValue)]
        public int RoundBreakDurationMinutes { get; set; } = 15;
    }
}
