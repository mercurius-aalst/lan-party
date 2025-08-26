namespace Mercurius.LAN.Web.DTOs.Participants.Players
{
    public class CreatePlayerDTO
    {
        public string Username { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string? DiscordId { get; set; }
        public string? SteamId { get; set; }
        public string? RiotId { get; set; }
        public string Email { get; set; } = null!;
    }
}
