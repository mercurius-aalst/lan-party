using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.DTOs.Participants.Players;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Microsoft.AspNetCore.Components.Forms;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class PlayerManagement
{
    private List<Player> _players = new();
    private List<UpdateAndCreatePlayerFormDTO> _displayPlayers = new();
    private UpdateAndCreatePlayerFormDTO _selectedPlayer = new();
    private bool _isCreateMode = true;
    private EditContext? _editContext;

    private CustomAutocomplete<UpdateAndCreatePlayerFormDTO> _autoCompleteComponent = null!;

    [Inject]
    private IParticipantService ParticipantService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
                _players = await ParticipantService.GetPlayersAsync();
                _displayPlayers = _players.Select(pl => new UpdateAndCreatePlayerFormDTO(pl)).ToList();
                await InvokeAsync(StateHasChanged);

            }
            catch(Exception)
            {
                ToastService.ShowError("Players could not be loaded.");
            }
        }
    }

    protected override void OnInitialized() => ReInitEditContext();
    private void ReInitEditContext()
    {
        _editContext = new(_selectedPlayer);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) =>
        {
            _editContext.Validate();
        };
    }

    private void OnPlayerSelected(UpdateAndCreatePlayerFormDTO player)
    {
        _selectedPlayer = player;
        _isCreateMode = false;
        ReInitEditContext();
    }

    private void ClearForm()
    {
        _selectedPlayer = new UpdateAndCreatePlayerFormDTO();
        _isCreateMode = true;
        _displayPlayers = _players.Select(pl => new UpdateAndCreatePlayerFormDTO(pl)).ToList();        
        _autoCompleteComponent.ClearSearchField();
        ReInitEditContext();
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
                await ParticipantService.UpdatePlayerAsync((int)_selectedPlayer.Id!, new UpdatePlayerDTO
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
            ToastService.ShowError(ex.Content!);
        }
    }

    private async Task DeletePlayer()
    {
        if(_selectedPlayer == null)
            return;
        try
        {
            await ParticipantService.DeletePlayerAsync((int)_selectedPlayer.Id!);
            _players.Remove(_players.Single(pl => pl.Id == _selectedPlayer.Id));
            ToastService.ShowSuccess("Player deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}