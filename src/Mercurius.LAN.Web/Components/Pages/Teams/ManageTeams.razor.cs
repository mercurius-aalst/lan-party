using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Teams;

public partial class ManageTeams : IAsyncDisposable
{
    private const long MaximumLogoBytes = 5 * 1024 * 1024;

    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private ITeamNotificationService NotificationService { get; set; } = null!;
    [Inject] private ITeamRealtimeService RealtimeService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private CurrentUserTeamSummaryDTO _summary = new();
    private bool _isLoading = true;
    private string? _loadError;
    private Guid? _selectedTeamId;
    private bool _isCreateTeamDialogOpen;
    private bool _isInviteDialogOpen;
    private Guid? _inviteTeamId;
    private string _inviteTeamName = "this team";
    private PublicUserDTO? _selectedUser;
    private ElementReference _confirmationDialogElement;
    private IJSObjectReference? _confirmationFocusTrap;
    private TeamConfirmation? _confirmation;
    private TeamManagementTab _activeTab = TeamManagementTab.Members;
    private readonly Dictionary<Guid, Guid?> _transferSelections = [];
    private readonly Dictionary<Guid, TeamLogoSelection> _selectedLogos = [];

    private IReadOnlyList<TeamManagementSummaryDTO> ManageableTeams =>
        _summary.CaptainedTeams
            .Concat(_summary.MemberTeams)
            .GroupBy(team => team.Id)
            .Select(group => group.FirstOrDefault(IsCaptain) ?? group.First())
            .ToList();

    private TeamManagementSummaryDTO? SelectedTeam =>
        _selectedTeamId.HasValue
            ? ManageableTeams.FirstOrDefault(team => team.Id == _selectedTeamId.Value)
            : null;

    protected override async Task OnInitializedAsync()
    {
        RealtimeService.TeamStateInvalidated += RefreshFromSignalAsync;
        await LoadSummaryAsync();

        try
        {
            await RealtimeService.StartAsync();
            await RealtimeService.JoinTeamsAsync(ManageableTeams.Select(team => team.Id));
        }
        catch(Exception)
        {
            ToastService.ShowWarning("Live updates are unavailable. Your changes will still appear after each action.");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_confirmation is not null && _confirmationFocusTrap is null)
        {
            _confirmationFocusTrap = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "activateTeamModalFocusTrap",
                _confirmationDialogElement);
        }
        else if(_confirmation is null && _confirmationFocusTrap is not null)
        {
            await DisposeConfirmationFocusTrapAsync();
        }
    }

    private async Task LoadSummaryAsync()
    {
        _isLoading = true;
        _loadError = null;
        try
        {
            _summary = await TeamService.GetCurrentUserTeamSummaryAsync();
            EnsureSelectedTeam();
            await NotificationService.RefreshAsync();
        }
        catch(Exception exception)
        {
            _summary = new();
            _loadError = GetErrorMessage(exception);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RefreshSummaryAsync()
    {
        _summary = await TeamService.GetCurrentUserTeamSummaryAsync();
        EnsureSelectedTeam();
        await NotificationService.RefreshAsync();
        await RealtimeService.JoinTeamsAsync(ManageableTeams.Select(team => team.Id));
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshFromSignalAsync()
    {
        try
        {
            await RefreshSummaryAsync();
        }
        catch(Exception)
        {
        }
    }

    private Task OpenCreateTeamDialogAsync()
    {
        _isCreateTeamDialogOpen = true;
        return Task.CompletedTask;
    }

    private void CloseCreateTeamDialog()
    {
        _isCreateTeamDialogOpen = false;
    }

    private async Task CreateTeamAsync(CreateTeamDialogResult createResult)
    {
        Team createdTeam;
        try
        {
            createdTeam = await TeamService.CreateTeamAsync(new CreateTeamDTO { Name = createResult.Name.Trim() });
        }
        catch(Exception exception)
        {
            ShowActionToast(GetErrorMessage(exception), TeamActionSeverity.Error);
            return;
        }

        _selectedTeamId = createdTeam.Id;

        if(createResult.Logo is null)
        {
            await RefreshSummaryAsync();
            _isCreateTeamDialogOpen = false;
            ToastService.ShowSuccess("Team created.");
            return;
        }

        try
        {
            await using var stream = new MemoryStream(createResult.Logo.Content);
            await TeamService.UploadLogoAsync(createdTeam.Id, stream, createResult.Logo.ContentType, createResult.Logo.FileName);
            await RefreshSummaryAsync();
            ToastService.ShowSuccess("Team created.");
        }
        catch(Exception exception)
        {
            await RefreshSummaryAsync();
            ShowActionToast($"Team created, but the logo could not be saved. {GetErrorMessage(exception)}", TeamActionSeverity.Error);
        }
        finally
        {
            _isCreateTeamDialogOpen = false;
        }
    }

    private Task OpenInviteDialogAsync(Guid teamId)
    {
        var team = ManageableTeams.FirstOrDefault(candidate => candidate.Id == teamId);
        _inviteTeamId = teamId;
        _inviteTeamName = team?.Name ?? "this team";
        _isInviteDialogOpen = true;
        return Task.CompletedTask;
    }

    private void CloseInviteDialog()
    {
        _isInviteDialogOpen = false;
        _inviteTeamId = null;
        _inviteTeamName = "this team";
    }

    private async Task InviteUserAsync(InviteUserDialogResult inviteResult)
    {
        if(!_inviteTeamId.HasValue)
            return;

        var teamId = _inviteTeamId.Value;
        CloseInviteDialog();
        await MutateAsync(async () =>
        {
            await TeamService.InviteUserAsync(teamId, inviteResult.UserId);
            await RefreshSummaryAsync();
            ShowActionToast("Invite sent.", TeamActionSeverity.Success);
        });
    }

    private async Task CancelInviteAsync(Guid teamId, Guid inviteId)
    {
        await MutateAsync(async () =>
        {
            await TeamService.CancelInviteAsync(teamId, inviteId);
            await RefreshSummaryAsync();
            ShowActionToast("Invite canceled.", TeamActionSeverity.Success);
        });
    }

    private async Task RespondInviteAsync(Guid inviteId, bool accept)
    {
        await MutateAsync(async () =>
        {
            await TeamService.RespondToInviteAsync(inviteId, accept);
            await RefreshSummaryAsync();
            ShowActionToast(accept ? "Invite accepted." : "Invite declined.", TeamActionSeverity.Success);
        });
    }

    private async Task ConfirmLeaveAsync(Guid teamId, string teamName)
    {
        _confirmation = new TeamConfirmation(
            "Membership",
            "Leave team",
            $"Leave {teamName}? You can join again if the captain invites you.",
            "Leave",
            async () =>
            {
                await MutateAsync(async () =>
                {
                    await TeamService.LeaveTeamAsync(teamId);
                    if(_selectedTeamId == teamId)
                        _selectedTeamId = null;

                    await RefreshSummaryAsync();
                    ShowActionToast("You left the team.", TeamActionSeverity.Success);
                });
            });
        await Task.CompletedTask;
    }

    private async Task ConfirmDeleteTeamAsync(Guid teamId, string teamName)
    {
        _confirmation = new TeamConfirmation(
            "Danger Zone",
            "Delete team",
            $"Delete {teamName}? This removes the team from your managed teams.",
            "Delete team",
            async () =>
            {
                await MutateAsync(async () =>
                {
                    await TeamService.DeleteTeamAsync(teamId);
                    _selectedLogos.Remove(teamId);
                    _transferSelections.Remove(teamId);
                    if(_selectedTeamId == teamId)
                        _selectedTeamId = null;

                    await RefreshSummaryAsync();
                    ShowActionToast("Team deleted.", TeamActionSeverity.Success);
                });
            });
        await Task.CompletedTask;
    }

    private async Task ConfirmRemoveMemberAsync(Guid teamId, string teamName, Guid userId, string memberName)
    {
        _confirmation = new TeamConfirmation(
            "Roster",
            "Remove member",
            $"Remove {memberName} from {teamName}?",
            "Remove member",
            async () =>
            {
                await MutateAsync(async () =>
                {
                    await TeamService.RemoveMemberAsync(teamId, userId);
                    if(GetTransferSelection(teamId) == userId)
                        _transferSelections.Remove(teamId);

                    await RefreshSummaryAsync();
                    ShowActionToast($"{memberName} removed from the team.", TeamActionSeverity.Success);
                });
            });
        await Task.CompletedTask;
    }

    private void CancelConfirmation()
    {
        _confirmation = null;
    }

    private Task HandleConfirmationKeyDown(KeyboardEventArgs args)
    {
        if(string.Equals(args.Key, "Escape", StringComparison.Ordinal))
            CancelConfirmation();

        return Task.CompletedTask;
    }

    private async Task ConfirmPendingActionAsync()
    {
        if(_confirmation is not { } confirmation)
            return;

        _confirmation = null;
        await confirmation.Action();
    }

    private async Task TransferCaptainAsync(Guid teamId)
    {
        var newCaptainUserId = GetTransferSelection(teamId);
        if(!newCaptainUserId.HasValue)
            return;

        await MutateAsync(async () =>
        {
            await TeamService.TransferCaptainAsync(teamId, newCaptainUserId.Value);
            _transferSelections.Remove(teamId);
            await RefreshSummaryAsync();
            ShowActionToast("Captainship transferred.", TeamActionSeverity.Success);
        });
    }

    private async Task SelectLogoAsync(Guid teamId, InputFileChangeEventArgs args)
    {
        var file = args.File;
        if(file.Size > MaximumLogoBytes)
        {
            ShowActionToast("Choose a logo no larger than 5 MB.", TeamActionSeverity.Warning);
            return;
        }

        if(!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ShowActionToast("Choose an image file for the team logo.", TeamActionSeverity.Warning);
            return;
        }

        var buffer = new byte[file.Size];
        await using var stream = file.OpenReadStream(MaximumLogoBytes);
        await stream.ReadExactlyAsync(buffer);
        var preview = $"data:{file.ContentType};base64,{Convert.ToBase64String(buffer)}";
        _selectedLogos[teamId] = new TeamLogoSelection(file.Name, file.ContentType, buffer, preview);
    }

    private async Task UploadLogoAsync(Guid teamId)
    {
        if(!_selectedLogos.TryGetValue(teamId, out var logo))
            return;

        await MutateAsync(async () =>
        {
            await using var stream = new MemoryStream(logo.Content);
            await TeamService.UploadLogoAsync(teamId, stream, logo.ContentType, logo.FileName);
            _selectedLogos.Remove(teamId);
            await RefreshSummaryAsync();
            ShowActionToast("Team logo saved.", TeamActionSeverity.Success);
        });
    }

    private async Task RemoveLogoAsync(Guid teamId)
    {
        await MutateAsync(async () =>
        {
            await TeamService.RemoveLogoAsync(teamId);
            _selectedLogos.Remove(teamId);
            await RefreshSummaryAsync();
            ShowActionToast("Team logo removed.", TeamActionSeverity.Success);
        });
    }

    private async Task MutateAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch(Exception exception)
        {
            ShowActionToast(GetErrorMessage(exception), TeamActionSeverity.Error);
        }
    }

    private void SelectTeam(Guid teamId)
    {
        _selectedTeamId = teamId;
        _activeTab = TeamManagementTab.Members;
    }

    private void SelectTab(TeamManagementTab tab) => _activeTab = tab;

    private void EnsureSelectedTeam()
    {
        var teams = ManageableTeams;
        if(teams.Count == 0)
        {
            _selectedTeamId = null;
            return;
        }

        if(!_selectedTeamId.HasValue || teams.All(team => team.Id != _selectedTeamId.Value))
            _selectedTeamId = teams[0].Id;

        if(SelectedTeam is { } selectedTeam && !IsCaptain(selectedTeam) && _activeTab == TeamManagementTab.Management)
            _activeTab = TeamManagementTab.Members;
    }

    private bool IsCaptain(TeamManagementSummaryDTO team) =>
        _summary.CaptainedTeams.Any(candidate => candidate.Id == team.Id);

    private string GetTeamSelectorClass(Guid teamId)
    {
        var classes = "team-selector-item";
        return _selectedTeamId == teamId ? $"{classes} team-selector-item--active" : classes;
    }

    private void SelectMemberParticipant(ParticipantViewModel participant)
    {
        if(participant.User is { } user)
            _selectedUser = user;
    }

    private void CloseUserInfoDialog() => _selectedUser = null;

    private string GetTabClass(TeamManagementTab tab)
    {
        var classes = "team-tab-button";
        return _activeTab == tab ? $"{classes} team-tab-button--active" : classes;
    }

    private static string BuildTeamProfileHref(string teamName) =>
        string.IsNullOrWhiteSpace(teamName)
            ? string.Empty
            : $"/teams/{Uri.EscapeDataString(teamName.Trim())}";

    private IReadOnlyList<TeamInviteSummaryDTO> GetSentInvites(Guid teamId) =>
        _summary.SentPendingInvites.Where(invite => invite.TeamId == teamId).ToList();

    private IReadOnlySet<Guid> GetInviteDisabledUserIds() =>
        _inviteTeamId.HasValue
            ? ManageableTeams
                .FirstOrDefault(team => team.Id == _inviteTeamId.Value)?
                .Members
                .Select(member => member.Id)
                .ToHashSet() ?? new HashSet<Guid>()
            : new HashSet<Guid>();

    private Guid? GetTransferSelection(Guid teamId) =>
        _transferSelections.TryGetValue(teamId, out var value) ? value : null;

    private void SetTransferSelection(Guid teamId, string? value) =>
        _transferSelections[teamId] = Guid.TryParse(value, out var parsed) ? parsed : null;

    private bool HasSelectedLogo(Guid teamId) => _selectedLogos.ContainsKey(teamId);

    private string? GetLogoPreview(Guid teamId) =>
        _selectedLogos.TryGetValue(teamId, out var logo) ? logo.PreviewDataUrl : null;

    private string GetLogoFileName(Guid teamId) =>
        _selectedLogos.TryGetValue(teamId, out var logo) ? logo.FileName : "No file selected";

    private void ShowActionToast(string message, TeamActionSeverity severity)
    {
        switch(severity)
        {
            case TeamActionSeverity.Success:
                ToastService.ShowSuccess(message);
                break;
            case TeamActionSeverity.Warning:
                ToastService.ShowWarning(message);
                break;
            case TeamActionSeverity.Error:
                ToastService.ShowError(message);
                break;
            default:
                ToastService.ShowInfo(message);
                break;
        }
    }

    private static string GetMemberName(PublicUserDTO member)
    {
        if(!string.IsNullOrWhiteSpace(member.Username))
            return member.Username.Trim();

        if(!string.IsNullOrWhiteSpace(member.DisplayName))
            return member.DisplayName.Trim();

        return "Player";
    }

    private static string GetInitials(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
    }

    private static string GetErrorMessage(Exception exception) =>
        exception is TeamServiceException serviceException
            ? serviceException.Message
            : "The team action could not be completed right now.";

    private async ValueTask DisposeConfirmationFocusTrapAsync()
    {
        var focusTrap = _confirmationFocusTrap;
        _confirmationFocusTrap = null;
        if(focusTrap is null)
            return;

        try
        {
            await focusTrap.InvokeVoidAsync("dispose");
            await focusTrap.DisposeAsync();
        }
        catch(JSDisconnectedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        RealtimeService.TeamStateInvalidated -= RefreshFromSignalAsync;
        await DisposeConfirmationFocusTrapAsync();
    }

    private enum TeamManagementTab
    {
        Members,
        Management
    }

    private enum TeamActionSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }

    private sealed record TeamConfirmation(
        string Eyebrow,
        string Title,
        string Message,
        string ConfirmLabel,
        Func<Task> Action);
}
