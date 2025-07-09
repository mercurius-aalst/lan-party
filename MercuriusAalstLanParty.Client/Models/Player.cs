namespace MercuriusAalstLanParty.Client.Models
{
    public class Player : Participant
    {
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string? DiscordId { get; set; } = string.Empty;
        public string? SteamId { get; set; } = string.Empty;
        public string? RiotId { get; set; } = string.Empty;
        public string Email { get; set; }
    }
}
