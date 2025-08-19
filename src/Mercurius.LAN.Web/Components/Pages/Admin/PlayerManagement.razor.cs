using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.DTOs.Participants.Players;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class PlayerManagement
{
    private List<Player> _players = new();
    private Player? _selectedPlayer;
    private bool _isCreateMode = true;

    private CustomAutocomplete<Player> _autoCompleteComponent = null!;

    [Inject]
    private IParticipantService ParticipantService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        try
        {
            _players = await ParticipantService.GetPlayersAsync();
        }
        catch(Exception)
        {
            ToastService.ShowError("Players could not be loaded.");
        }
    }

    private void OnPlayerSelected(Player player)
    {
        _selectedPlayer = player;
        _isCreateMode = false;
    }

    private void ClearForm()
    {
        _selectedPlayer = new Player();
        _isCreateMode = true;
        _autoCompleteComponent.ClearSearchField();
        StateHasChanged();
    }

    private async Task HandleSubmit()
    {
        try
        {
            if(_isCreateMode)
            {
                var player = await ParticipantService.CreatePlayerAsync(new CreatePlayerDTO
                {
                    Username = _selectedPlayer!.Username,
                    Firstname = _selectedPlayer.Firstname,
                    Lastname = _selectedPlayer.Lastname,
                    Email = _selectedPlayer.Email,
                    DiscordId = _selectedPlayer.DiscordId,
                    SteamId = _selectedPlayer.SteamId,
                    RiotId = _selectedPlayer.RiotId
                });
                _players.Add(player);
                ToastService.ShowSuccess("Player created successfully.");
            }
            else
            {
                await ParticipantService.UpdatePlayerAsync(_selectedPlayer!.Id, new UpdatePlayerDTO
                {
                    Username = _selectedPlayer.Username,
                    Firstname = _selectedPlayer.Firstname,
                    Lastname = _selectedPlayer.Lastname,
                    DiscordId = _selectedPlayer.DiscordId,
                    SteamId = _selectedPlayer.SteamId,
                    RiotId = _selectedPlayer.RiotId
                });
                ToastService.ShowSuccess("Player updated successfully.");
            }
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }

    private async Task DeletePlayer()
    {
        if(_selectedPlayer == null)
            return;
        try
        {
            await ParticipantService.DeletePlayerAsync(_selectedPlayer.Id);
            _players.Remove(_selectedPlayer);
            ToastService.ShowSuccess("Player deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }
}