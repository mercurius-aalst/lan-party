using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class TeamManagement
{
    private List<Team> _teams = new();
    private List<Player> _players = new();
    private Team? _selectedTeam;
    private Player? _selectedCaptain;
    private bool _isCreateMode = true;

    private CustomAutocomplete<Team> _autoCompleteComponent = null!;

    [Inject]
    private IParticipantService ParticipantService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;


    protected override async Task OnAfterRenderAsync(bool firstRender){
        if(firstRender)
        {
            try
            {
                _players = await ParticipantService.GetPlayersAsync();
                _teams = await ParticipantService.GetTeamsAsync();
            }
            catch(Exception)
            {
                ToastService.ShowError("Teams could not be loaded.");
            }
        }
    }

    private void OnTeamSelected(Team team)
    {
        _selectedTeam = team;
        _selectedCaptain = _players.FirstOrDefault(p => p.Id == team.CaptainId);
        _isCreateMode = false;
    }

    private void OnCaptainSelected(Player player)
    {
        if(_selectedTeam != null)
        {
            _selectedCaptain = player;
            _selectedTeam.CaptainId = player.Id;
        }
    }

    private void ClearForm()
    {
        _selectedTeam = new Team();
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
                var team = await ParticipantService.CreateTeamAsync(new CreateTeamDTO
                {
                    Name = _selectedTeam!.Name,
                    CaptainId = _selectedTeam.CaptainId
                });
                _teams.Add(team);
                ToastService.ShowSuccess("Team created successfully.");
            }
            else
            {
                await ParticipantService.UpdateTeamAsync(_selectedTeam!.Id, new UpdateTeamDTO
                {
                    Name = _selectedTeam.Name,
                    CaptainId = _selectedTeam.CaptainId
                });
                ToastService.ShowSuccess("Team updated successfully.");
            }
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }

    private async Task DeleteTeam()
    {
        if(_selectedTeam == null)
            return;
        try
        {
            await ParticipantService.DeleteTeamAsync(_selectedTeam.Id);
            _teams.Remove(_selectedTeam);
            ToastService.ShowSuccess("Team deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }
}