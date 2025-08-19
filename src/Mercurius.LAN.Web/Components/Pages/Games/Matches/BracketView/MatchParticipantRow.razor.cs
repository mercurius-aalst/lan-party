using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class MatchParticipantRow
{
    [Parameter]
    public IEnumerable<Participant> Participants { get; set; } = Enumerable.Empty<Participant>();
    [Parameter]
    public Match Match { get; set; } = null!;
    [Parameter]
    public int? ParticipantId { get; set; }
    [Parameter]
    public int SequenceNumber { get; set; }
    [Parameter]
    public string ParticipantName { get; set; } = string.Empty;

    private bool _isWinner = false;

    protected override void OnParametersSet()
    {
        _isWinner = (Match.WinnerId is not null && Match.WinnerId == ParticipantId);
        base.OnParametersSet();
    }

    private string CalculateParticipantScore(Match match, int participantNumber)
    {
        var participantScore = participantNumber switch
        {
            1 => match.Participant1Score ?? 0,
            2 => match.Participant2Score ?? 0,
            _ => 0
        };
        return $"[{participantScore}]";
    }
}