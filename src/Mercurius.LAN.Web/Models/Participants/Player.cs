namespace Mercurius.LAN.Web.Models.Participants
{
    public class Player : Participant
    {
        public string Username { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string? DiscordId { get; set; } = string.Empty;
        public string? SteamId { get; set; } = string.Empty;
        public string? RiotId { get; set; } = string.Empty;
        public string Email { get; set; } = null!;

        public override ParticipantType Type => ParticipantType.Player;
    }
}