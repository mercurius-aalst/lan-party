// Moved @code block from MatchDetailsDialog.razor
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;
using Blazored.Toast.Services;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.DetailView;

public partial class MatchDetailsDialog
{
    [Parameter]
    public Match Match { get; set; } = null!;
    [Parameter]
    public IEnumerable<Participant> Participants { get; set; } = null!;
    [Parameter]
    public EventCallback OnClose { get; set; }
    [Parameter]
    public EventCallback OnDataReload { get; set; }
    [Parameter]
    public string Participant2Name { get; set; } = null!;
    [Parameter]
    public string Participant1Name { get; set; } = null!;
    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private Participant GetParticipantById(int? participantId)
    {
        if(participantId == null || Participants == null)
            return null!;
        return Participants.FirstOrDefault(p => p.Id == participantId.Value)!;
    }

    private bool IsWinner(int? participantId) => Match.WinnerId != null && participantId == Match.WinnerId;

    private bool IsLoser(int? participantId, int? opponentId)
    {
        if(Match.WinnerId == null)
            return false;
        if(participantId is null || participantId != Match.WinnerId)
            return true;
        return false;
    }

    private string GetCardClass(int? participantId, int? opponentId)
    {
        if(IsWinner(participantId))
            return "participant-card winner-card";
        if(IsLoser(participantId, opponentId))
            return "participant-card loser-card";
        return "participant-card";
    }

    private async Task SaveScoresAsync()
    {
        try
        {
            Match = await GameService.UpdateMatchScoresAsync(Match.Id, new UpdateMatchDTO()
            {
                Participant1Score = Match.Participant1Score ?? 0,
                Participant2Score = Match.Participant2Score ?? 0,
            });
            ToastService.ShowSuccess("Score updated successfully");
            await OnDataReload.InvokeAsync(); // Notify parent to reload data
            await OnClose.InvokeAsync(); // Close the dialog
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}