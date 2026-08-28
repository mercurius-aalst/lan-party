using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.DetailView;

public partial class TournamentMatchDetailsDialog
{
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
    [Parameter] public string Participant2Name { get; set; } = null!;
    [Parameter] public string Participant1Name { get; set; } = null!;
    [Parameter] public ParticipantViewModel? Participant1 { get; set; }
    [Parameter] public ParticipantViewModel? Participant2 { get; set; }

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private Guid? Participant1Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant1Id : Match.UserParticipant1Id;
    private Guid? Participant2Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant2Id : Match.UserParticipant2Id;
    private Guid? WinnerId => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamWinnerId : Match.UserWinnerId;
    private ParticipantViewModel? HeaderParticipant1 => Participant1 ?? GetParticipantById(Participant1Id);
    private ParticipantViewModel? HeaderParticipant2 => Participant2 ?? GetParticipantById(Participant2Id);
    private TournamentParticipantLookup _participantLookup = TournamentParticipantLookup.Empty;

    protected override void OnParametersSet()
    {
        _participantLookup = TournamentParticipantLookup.FromTournament(Tournament);
    }

    private ParticipantViewModel? GetParticipantById(Guid? participantId)
    {
        return _participantLookup.Resolve(Match.ParticipationMode, participantId);
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

    private static string BuildTeamProfileHref(string teamName) =>
        string.IsNullOrWhiteSpace(teamName)
            ? string.Empty
            : $"/teams/{Uri.EscapeDataString(teamName.Trim())}";

    private async Task SaveScoresAsync()
    {
        try
        {
            Match = await TournamentService.UpdateMatchScoresAsync(Match.Id, new UpdateMatchDTO
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
