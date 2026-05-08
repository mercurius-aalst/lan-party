using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.DTOs.Users;

public class CompleteUserProfileRequest
{
    [Required]
    [RegularExpression("^[a-zA-Z0-9]{3,32}$", ErrorMessage = "Username must be 3-32 alphanumeric characters.")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Firstname { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Lastname { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DiscordId { get; set; }

    [StringLength(100)]
    public string? SteamId { get; set; }

    [StringLength(100)]
    public string? RiotId { get; set; }
}
