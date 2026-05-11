using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class TeamManagement
{
    private List<Team> _teams = new();
    private List<UserDTO> _users = new();
    private Team _selectedTeam = new();
    private UserDTO? _selectedCaptain;
    private bool _isCreateMode = true;
    private bool _isLoading = true;
    private EditContext? _editContext;
    private CustomAutocomplete<Team> _autoCompleteComponent = null!;

    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    protected override void OnInitialized()
    {
        ReInitEditContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
                _users = (await UserClient.GetAllUsersAsync()).ToList();
                _teams = await TeamService.GetTeamsAsync();
            }
            catch(Exception)
            {
                ToastService.ShowError("Teams could not be loaded.");
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void ReInitEditContext()
    {
        _editContext = new(_selectedTeam);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => _editContext.Validate();
    }

    private void OnTeamSelected(Team team)
    {
        _selectedTeam = new Team
        {
            Id = team.Id,
            Name = team.Name,
            CaptainUserId = team.CaptainUserId,
            Members = team.Members.ToList(),
            TeamInvites = team.TeamInvites.ToList()
        };
        _selectedCaptain = _users.FirstOrDefault(user => user.Id == team.CaptainUserId);
        _isCreateMode = false;
        ReInitEditContext();
    }

    private void OnCaptainSelected(UserDTO user)
    {
        _selectedCaptain = user;
        _selectedTeam.CaptainUserId = user.Id;
    }

    private void ClearForm()
    {
        _selectedTeam = new Team();
        _selectedCaptain = null;
        _isCreateMode = true;
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
                var team = await TeamService.CreateTeamAsync(new CreateTeamDTO
                {
                    Name = _selectedTeam.Name,
                    CaptainUserId = _selectedTeam.CaptainUserId
                });
                _teams.Add(team);
                _selectedTeam = team;
                _selectedCaptain = _users.FirstOrDefault(user => user.Id == team.CaptainUserId);
                _isCreateMode = false;
                ReInitEditContext();
                await InvokeAsync(StateHasChanged);
                ToastService.ShowSuccess("Team created successfully.");
            }
            else
            {
                var updatedTeam = await TeamService.UpdateTeamAsync(_selectedTeam.Id, new UpdateTeamDTO
                {
                    Name = _selectedTeam.Name,
                    CaptainUserId = _selectedTeam.CaptainUserId
                });

                var existingIndex = _teams.FindIndex(team => team.Id == updatedTeam.Id);
                if(existingIndex >= 0)
                {
                    _teams[existingIndex] = updatedTeam;
                }

                _selectedTeam = updatedTeam;
                _selectedCaptain = _users.FirstOrDefault(user => user.Id == updatedTeam.CaptainUserId);
                ReInitEditContext();
                ToastService.ShowSuccess("Team updated successfully.");
            }
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private async Task DeleteTeam()
    {
        try
        {
            await TeamService.DeleteTeamAsync(_selectedTeam.Id);
            _teams.RemoveAll(team => team.Id == _selectedTeam.Id);
            ToastService.ShowSuccess("Team deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private static string GetUserLabel(UserDTO user)
    {
        return user.Username ?? user.DisplayName;
    }
}
