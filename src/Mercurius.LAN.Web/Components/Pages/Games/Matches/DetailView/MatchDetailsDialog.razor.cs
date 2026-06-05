using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.DetailView;

public partial class MatchDetailsDialog
{
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
    [Parameter] public string Participant2Name { get; set; } = null!;
    [Parameter] public string Participant1Name { get; set; } = null!;

    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private Guid? Participant1Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant1Id : Match.UserParticipant1Id;
    private Guid? Participant2Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant2Id : Match.UserParticipant2Id;
    private Guid? WinnerId => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamWinnerId : Match.UserWinnerId;

    private ParticipantViewModel? GetParticipantById(Guid? participantId)
    {
        if(participantId is null)
            return null;

        return Match.ParticipationMode switch
        {
            ParticipationMode.Individual => Game.Users.Where(user => user.Id == participantId.Value).Select(ParticipantViewModel.FromUser).FirstOrDefault(),
            ParticipationMode.Team => Game.Teams.Where(team => team.Id == participantId.Value).Select(ParticipantViewModel.FromTeam).FirstOrDefault(),
            _ => null
        };
    }

    private bool IsWinner(Guid? participantId) => WinnerId != null && participantId == WinnerId;

    private string GetStageLabel() => Match.IsLowerBracketMatch ? "Lower bracket" : "Main bracket";

    private string GetRoundLabel() => $"Round {Match.RoundNumber}";

    private string GetStatusLabel() => WinnerId != null ? "Decided" : Match.EstimatedStartTime.HasValue ? "Estimated" : "Awaiting estimate";

    private string GetStatusClass() => WinnerId != null ? "match-status-pill--complete" : Match.EstimatedStartTime.HasValue ? "match-status-pill--scheduled" : "match-status-pill--pending";

    private string GetStartDateTimeLabel() =>
        Match.EstimatedStartTime.HasValue
            ? Match.EstimatedStartTime.Value.ToLocalDisplayTime().ToString("dd MMM yyyy · HH:mm")
            : "Unavailable";

    private string GetCardClass(Guid? participantId)
    {
        if(IsWinner(participantId))
            return "participant-card winner-card";
        if(WinnerId != null && participantId != null)
            return "participant-card loser-card";
        return "participant-card";
    }

    private async Task SaveScoresAsync()
    {
        try
        {
            Match = await GameService.UpdateMatchScoresAsync(Match.Id, new UpdateMatchDTO
            {
                Participant1Score = Match.Participant1Score ?? 0,
                Participant2Score = Match.Participant2Score ?? 0,
            });
            ToastService.ShowSuccess("Score updated successfully");
            await OnDataReload.InvokeAsync();
            await OnClose.InvokeAsync();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}
