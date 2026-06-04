using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Games
{
    public class CreateGameDTO : IValidatableObject
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
        public int AverageGameDurationMinutes { get; set; } = 30;

        [Range(1, int.MaxValue)]
        public int RoundBreakDurationMinutes { get; set; } = 10;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(PlannedStartTime == default)
            {
                yield return new ValidationResult(
                    "Planned start time is required.",
                    [nameof(PlannedStartTime)]);
            }
        }
    }
}
