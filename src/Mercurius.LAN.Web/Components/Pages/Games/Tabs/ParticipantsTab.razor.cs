using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class ParticipantsTab
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback<GameExtended> OnGameUpdated { get; set; }

    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private ParticipantViewModel? _selectedParticipant;
    private bool _isAddParticipantsPopupVisible;
    private List<ParticipantViewModel> _availableParticipants = new();
    private List<ParticipantViewModel> _participants = new();

    protected override void OnParametersSet()
    {
        _participants = BuildParticipants(Game).ToList();
    }

    private static IEnumerable<ParticipantViewModel> BuildParticipants(GameExtended game)
    {
        return game.ParticipationMode switch
        {
            ParticipationMode.Individual => game.Users.Select(ParticipantViewModel.FromUser),
            ParticipationMode.Team => game.Teams.Select(ParticipantViewModel.FromTeam),
            _ => Enumerable.Empty<ParticipantViewModel>()
        };
    }

    private void DisplayParticipantPopup(ParticipantViewModel participant)
    {
        _selectedParticipant = participant;
    }

    private void HidePopup()
    {
        _selectedParticipant = null;
    }

    private async Task DisplayAddParticipantsPopupAsync()
    {
        IEnumerable<ParticipantViewModel> allParticipants = Game.ParticipationMode switch
        {
            ParticipationMode.Individual => (await UserClient.GetAllUsersAsync()).Select(ParticipantViewModel.FromUser),
            ParticipationMode.Team => (await TeamService.GetTeamsAsync()).Select(ParticipantViewModel.FromTeam),
            _ => Enumerable.Empty<ParticipantViewModel>()
        };

        _availableParticipants = allParticipants
            .Where(candidate => _participants.All(existing => existing.Id != candidate.Id))
            .ToList();

        if(_availableParticipants.Any())
        {
            _isAddParticipantsPopupVisible = true;
        }
        else
        {
            ToastService.ShowInfo($"No available {GetParticipantLabel()} to add");
        }
    }

    private async Task AddParticipantAsync(ParticipantViewModel participant)
    {
        try
        {
            var updatedGame = Game.ParticipationMode switch
            {
                ParticipationMode.Individual => await GameService.RegisterUserForGameAsync(Game.Id, participant.Id),
                ParticipationMode.Team => await GameService.RegisterTeamForGameAsync(Game.Id, participant.Id),
                _ => Game
            };

            await ApplyUpdatedGameAsync(updatedGame);
            _availableParticipants.RemoveAll(item => item.Id == participant.Id);

            if(!_availableParticipants.Any())
            {
                _isAddParticipantsPopupVisible = false;
            }

            ToastService.ShowSuccess($"{participant.DisplayName} has been added to the game.");
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

    private async Task RemoveParticipantAsync(ParticipantViewModel participant)
    {
        try
        {
            var updatedGame = Game.ParticipationMode switch
            {
                ParticipationMode.Individual => await GameService.UnregisterUserFromGameAsync(Game.Id, participant.Id),
                ParticipationMode.Team => await GameService.UnregisterTeamFromGameAsync(Game.Id, participant.Id),
                _ => Game
            };

            await ApplyUpdatedGameAsync(updatedGame);
            ToastService.ShowSuccess($"{participant.DisplayName} has been removed from the game.");
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private void ShowParticipantPopup(ParticipantViewModel participant)
    {
        DisplayParticipantPopup(participant);
    }

    private Task UnregisterParticipantAsync(ParticipantViewModel participant)
    {
        return RemoveParticipantAsync(participant);
    }

    private void ClosePopup()
    {
        HidePopup();
    }

    private string GetParticipantLabel()
    {
        return Game.ParticipationMode == ParticipationMode.Team ? "teams" : "users";
    }

    private string GetParticipantsHeading()
    {
        return Game.ParticipationMode == ParticipationMode.Team ? "teams" : "players";
    }

    private async Task ApplyUpdatedGameAsync(GameExtended updatedGame)
    {
        Game = updatedGame;
        _participants = BuildParticipants(updatedGame).ToList();
        await OnGameUpdated.InvokeAsync(updatedGame);
    }
}
