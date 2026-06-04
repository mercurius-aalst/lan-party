namespace Mercurius.LAN.Web.Models.Games
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public int AverageGameDurationMinutes { get; set; }
        public int RoundBreakDurationMinutes { get; set; }
        public DateTime? EstimatedEndTime { get; set; }
        public string? ImageUrl { get; set; }
        public GameStatus Status { get; set; }
        public BracketType BracketType { get; set; }
        public GameFormat Format { get; set; }
        public GameFormat FinalsFormat { get; set; }
        public ParticipationMode ParticipationMode { get; set; }
        public string RegisterFormUrl { get; set; } = null!;
    }
}
