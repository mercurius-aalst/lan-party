namespace Mercurius.LAN.Web.Models
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BracketType BracketType { get; set; }
        public GameFormat Format { get; set; }
        public ParticipantType ParticipantType { get; set; }
        public int RoundNumber { get; set; }
        public int MatchNumber { get; set; }
        public bool IsLowerBracketMatch { get; set; }
        public int GameId { get; set; }
        public int? Participant1Id { get; set; }
        public int? Participant2Id { get; set; }
        public int? WinnerId { get; set; }
        public int? Participant1Score { get; set; }
        public int? Participant2Score { get; set; }
        public int? WinnerNextMatchId { get; set; }
        public int? LoserNextMatchId { get; set; }
    }
}
