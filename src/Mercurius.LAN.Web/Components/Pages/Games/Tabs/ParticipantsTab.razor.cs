using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;
using Blazored.Toast.Services;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class ParticipantsTab
{
    [Parameter]
    public IEnumerable<Participant> Participants { get; set; } = Enumerable.Empty<Participant>();
    [Parameter]
    public ParticipantType ParticipantType { get; set; }
    [Parameter]
    public Game Game { get; set; } = null!;

    [Inject]
    private IParticipantService ParticipantService { get; set; } = null!;
    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private Participant? _selectedParticipant;
    private bool _isAddParticipantsPopupVisible;
    private List<Participant> _availableParticipants = new();

    private void DisplayParticipantPopup(Participant participant)
    {
        _selectedParticipant = participant;
    }

    private void HidePopup()
    {
        _selectedParticipant = null;
    }

    private async Task DisplayAddParticipantsPopupAsync()
    {
        IEnumerable<Participant> allParticipants = Enumerable.Empty<Participant>();
        switch(ParticipantType)
        {
            case ParticipantType.Player:
                allParticipants = await ParticipantService.GetPlayersAsync();
                break;
            case ParticipantType.Team:
                allParticipants = await ParticipantService.GetTeamsAsync();
                break;
        }

        _availableParticipants = allParticipants
            .Where(ap => !Participants.Any(p => p.Id == ap.Id))
            .ToList();

        if(_availableParticipants.Any())
        {
            _isAddParticipantsPopupVisible = true;
        }
        else
        {
            ToastService.ShowInfo($"No available {ParticipantType}s to add");
        }
    }

    private async Task AddParticipantAsync(Participant participant)
    {
        try
        {
            var updatedGame = await GameService.RegisterForGameAsync(Game.Id, participant.Id);
            Participants = updatedGame.Participants;
            _availableParticipants.Remove(participant);

            var participantName = participant is Player player ? player.Username : (participant as Team)?.Name ?? "Unknown";

            if(!_availableParticipants.Any())
            {
                _isAddParticipantsPopupVisible = false;
            }

            ToastService.ShowSuccess($"{participantName} has been added to the game.");
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private void HideAddParticipantsPopup()
    {
        _isAddParticipantsPopupVisible = false;
        _availableParticipants.Clear();
    }

    private async Task RemoveParticipantAsync(Participant participant)
    {
        try
        {
            var updatedGame = await GameService.UnregisterFromGameAsync(Game.Id, participant.Id);
            Participants = updatedGame.Participants;

            var participantName = participant is Player player ? player.Username : (participant as Team)?.Name ?? "Unknown";
            ToastService.ShowSuccess($"{participantName} has been removed from the game.");
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private void ShowParticipantPopup(Participant participant)
    {
        DisplayParticipantPopup(participant);
    }

    private async Task UnregisterParticipantAsync(Participant participant)
    {
        await RemoveParticipantAsync(participant);
    }

    private void ClosePopup()
    {
        HidePopup();
    }
}