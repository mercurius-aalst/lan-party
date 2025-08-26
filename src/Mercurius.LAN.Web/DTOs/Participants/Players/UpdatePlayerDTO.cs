namespace Mercurius.LAN.Web.DTOs.Participants.Players
{
    public class UpdatePlayerDTO
    {
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? DiscordId { get; set; }
        public string? SteamId { get; set; }
        public string? RiotId { get; set; }
    }
}
