namespace Mercurius.LAN.Web.DTOs.PublicProfiles;

public class PublicUserProfileDTO
{
    public string Username { get; set; } = string.Empty;
    public string? DiscordId { get; set; }
    public string? SteamId { get; set; }
    public string? RiotId { get; set; }
}
