namespace Mercurius.LAN.Web.DTOs.Matches;

public sealed class MatchActionStateDTO
{
    public Models.Matches.Match Match { get; set; } = null!;
    public MatchParticipantSide? AuthorizedParticipant { get; set; }
    public bool CanConfirmEnded { get; set; }
    public bool CanSubmitScore { get; set; }
    public bool CanForfeit { get; set; }
    public int? Participant1ReportedScore1 { get; set; }
    public int? Participant1ReportedScore2 { get; set; }
    public int? Participant2ReportedScore1 { get; set; }
    public int? Participant2ReportedScore2 { get; set; }
}
