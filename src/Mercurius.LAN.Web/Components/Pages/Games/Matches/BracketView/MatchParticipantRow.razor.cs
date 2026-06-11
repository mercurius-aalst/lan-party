using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;
using Mercurius.LAN.Web.Components.Shared;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class MatchParticipantRow
{
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public Guid? ParticipantId { get; set; }
    [Parameter] public int SequenceNumber { get; set; }
    [Parameter] public string ParticipantName { get; set; } = string.Empty;
    [Parameter] public ParticipantViewModel? Participant { get; set; }

    private bool _isWinner;

    protected override void OnParametersSet()
    {
        var winnerId = Match.ParticipationMode == ParticipationMode.Team ? Match.TeamWinnerId : Match.UserWinnerId;
        _isWinner = winnerId is not null && winnerId == ParticipantId;
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
