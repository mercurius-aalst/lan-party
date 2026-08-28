using System.Net;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentParticipantsTab
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<TournamentExtended> OnTournamentUpdated { get; set; }

    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private readonly List<ParticipantViewModel> _participants = [];
    private readonly List<AdminTournamentRegistrationDTO> _adminRegistrations = [];
    private readonly HashSet<Guid> _selectedRosterUserIds = [];

    private ParticipantViewModel? _selectedParticipant;
    private CurrentUserTournamentRegistrationStateDTO? _registrationState;
    private EligibilityResponseDTO? _individualEligibility;
    private EligibilityResponseDTO? _selectedTeamEligibility;
    private RosterCandidateEligibilityResponseDTO? _rosterEligibility;
    private CurrentUserTeamSummaryDTO? _teamSummary;
    private Guid? _selectedTeamId;
    private Guid? _pendingAdminRemovalRegistrationId;
    private string _adminRemovalReason = string.Empty;
    private string? _registrationError;
    private string? _adminError;
    private string? _teamError;
    private bool _isAuthenticated;
    private bool _isAdmin;
    private bool _isLoadingRegistration;
    private bool _isLoadingTeams;
    private bool _isLoadingAdmin;
    private bool _isSubmitting;
    private bool _hasLoadedForTournament;
    private Guid _loadedTournamentId;

    private IReadOnlyList<TeamManagementSummaryDTO> CaptainedTeams =>
        _teamSummary?.CaptainedTeams ?? [];

    private TeamManagementSummaryDTO? SelectedTeam =>
        _selectedTeamId.HasValue
            ? CaptainedTeams.FirstOrDefault(team => team.Id == _selectedTeamId.Value)
            : null;

    private int RequiredTeamSize => Tournament.TeamSize.GetValueOrDefault();

    private bool IsRegistrationOpen => Tournament.Status == TournamentStatus.Scheduled;

    private bool CanSubmitRoster =>
        IsRegistrationOpen &&
        SelectedTeam is not null &&
        RequiredTeamSize > 0 &&
        _selectedRosterUserIds.Count == RequiredTeamSize &&
        _selectedRosterUserIds.Contains(SelectedTeam.CaptainUserId) &&
        (_rosterEligibility?.Eligible ?? false);

    private bool HasPublicParticipants => _participants.Count > 0;

    protected override void OnParametersSet()
    {
        if(_loadedTournamentId != Tournament.Id)
        {
            _loadedTournamentId = Tournament.Id;
            _hasLoadedForTournament = false;
            ResetRegistrationContext();
        }

        _participants.Clear();
        _participants.AddRange(BuildParticipants(Tournament));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender || _hasLoadedForTournament || Tournament.Id == Guid.Empty)
            return;

        _hasLoadedForTournament = true;
        await LoadRegistrationContextAsync();
    }

    private static IEnumerable<ParticipantViewModel> BuildParticipants(TournamentExtended tournament)
    {
        var seen = new HashSet<Guid>();

        foreach(var registration in tournament.Registrations ?? [])
        {
            if(registration.Status != TournamentRegistrationStatus.Active)
                continue;

            if(registration.Kind == TournamentRegistrationKind.Individual && registration.User is not null)
            {
                if(seen.Add(registration.User.Id))
                    yield return ParticipantViewModel.FromUser(registration.User);

                continue;
            }

            if(registration.Kind == TournamentRegistrationKind.Team && registration.Team is not null && seen.Add(registration.Team.Id))
            {
                yield return ParticipantViewModel.FromTeam(new Team
                {
                    Id = registration.Team.Id,
                    Name = registration.Team.Name,
                    CaptainUserId = registration.Team.CaptainUserId,
                    LogoUrl = registration.Team.LogoUrl,
                    Members = registration.RosterMembers
                        .Where(member => member.User is not null)
                        .Select(member => member.User)
                        .ToList()
                });
            }
        }
    }

    private async Task LoadRegistrationContextAsync()
    {
        _isLoadingRegistration = true;
        _registrationError = null;
        _adminError = null;
        _teamError = null;

        try
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var principal = authenticationState.User;
            _isAuthenticated = principal.Identity?.IsAuthenticated == true;
            _isAdmin = _isAuthenticated && principal.IsInRole("admin");

            if(!_isAuthenticated)
                return;

            try
            {
                _registrationState = await TournamentService.GetCurrentUserTournamentRegistrationStateAsync(Tournament.Id);
            }
            catch(Exception exception) when(IsUnauthorized(exception))
            {
                _registrationError = "Your account is not authorized to view registration status.";
            }

            if(IsRegistrationOpen)
            {
                if(Tournament.ParticipationMode == ParticipationMode.Individual)
                    await LoadIndividualEligibilityAsync();
                else
                    await LoadTeamRegistrationContextAsync();
            }

            if(_isAdmin)
                await LoadAdminRegistrationsAsync();
        }
        catch(Exception exception)
        {
            _registrationError = GetErrorMessage(exception, "Registration options are unavailable right now.");
        }
        finally
        {
            _isLoadingRegistration = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadIndividualEligibilityAsync()
    {
        try
        {
            _individualEligibility = await TournamentService.CheckIndividualTournamentRegistrationEligibilityAsync(Tournament.Id);
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _registrationError ??= "Your account is not authorized to check individual registration eligibility.";
        }
        catch(Exception exception)
        {
            _registrationError ??= GetErrorMessage(exception, "Individual registration eligibility is unavailable right now.");
        }
    }

    private async Task LoadTeamRegistrationContextAsync()
    {
        _isLoadingTeams = true;
        try
        {
            _teamSummary = await TeamService.GetCurrentUserTeamSummaryAsync();
            if(_selectedTeamId.HasValue && CaptainedTeams.All(team => team.Id != _selectedTeamId.Value))
                _selectedTeamId = null;

            if(!_selectedTeamId.HasValue)
                _selectedTeamId = CaptainedTeams.FirstOrDefault()?.Id;

            if(_selectedTeamId.HasValue)
                await LoadSelectedTeamEligibilityAsync();
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _teamError = "Sign in with a team captain account to submit a team roster.";
        }
        catch(Exception exception)
        {
            _teamError = GetErrorMessage(exception, "Your team registration options are unavailable right now.");
        }
        finally
        {
            _isLoadingTeams = false;
        }
    }

    private async Task LoadSelectedTeamEligibilityAsync()
    {
        _selectedTeamEligibility = null;
        _rosterEligibility = null;
        _selectedRosterUserIds.Clear();
        _teamError = null;

        if(SelectedTeam is null)
            return;

        try
        {
            _selectedTeamEligibility = await TournamentService.CheckTeamTournamentRegistrationEligibilityAsync(
                Tournament.Id,
                SelectedTeam.Id);

            if(_registrationState?.CaptainManagedRegistrations.FirstOrDefault(registration => registration.Team?.Id == SelectedTeam.Id) is { } registration)
            {
                foreach(var member in registration.RosterMembers)
                    _selectedRosterUserIds.Add(member.User.Id);
            }
            else
            {
                foreach(var member in SelectedTeam.Members)
                    _selectedRosterUserIds.Add(member.Id);
            }

            await RefreshRosterEligibilityAsync();
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _teamError = "You are not authorized to manage this team registration.";
        }
        catch(Exception exception)
        {
            _teamError = GetErrorMessage(exception, "Team registration eligibility is unavailable right now.");
        }
    }

    private async Task RefreshRosterEligibilityAsync()
    {
        _rosterEligibility = null;
        if(SelectedTeam is null || _selectedRosterUserIds.Count == 0)
            return;

        try
        {
            _rosterEligibility = await TournamentService.CheckTeamRosterEligibilityAsync(
                Tournament.Id,
                SelectedTeam.Id,
                BuildRosterRequest());
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _teamError = "You are not authorized to validate this roster.";
        }
        catch(Exception exception)
        {
            _teamError = GetErrorMessage(exception, "Roster eligibility is unavailable right now.");
        }
    }

    private async Task LoadAdminRegistrationsAsync()
    {
        _isLoadingAdmin = true;
        try
        {
            var registrations = await TournamentService.GetAdminTournamentRegistrationsAsync(Tournament.Id, 1, 100);
            _adminRegistrations.Clear();
            _adminRegistrations.AddRange(registrations);
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _adminError = "You are not authorized to view the registration administration list.";
        }
        catch(Exception exception)
        {
            _adminError = GetErrorMessage(exception, "The administration registration list is unavailable right now.");
        }
        finally
        {
            _isLoadingAdmin = false;
        }
    }

    private void DisplayParticipantPopup(ParticipantViewModel participant)
    {
        _selectedParticipant = participant;
    }

    private void HidePopup()
    {
        _selectedParticipant = null;
    }

    private async Task RegisterIndividualAsync()
    {
        if(!IsRegistrationOpen ||
           _isSubmitting ||
           _registrationState?.IndividualRegistration is not null ||
           !(_registrationState?.CanRegisterIndividual ?? false) ||
           !(_individualEligibility?.Eligible ?? false))
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.RegisterCurrentUserForTournamentAsync(Tournament.Id),
            "You are registered for this tournament.");
    }

    private async Task UnregisterIndividualAsync()
    {
        if(!IsRegistrationOpen || _isSubmitting)
            return;

        await RunRegistrationActionAsync(
            async () =>
            {
                await TournamentService.DeleteCurrentUserTournamentRegistrationAsync(Tournament.Id);
            },
            "Your tournament registration was removed.");
    }

    private async Task ConfirmRosterMemberAsync()
    {
        if(!IsRegistrationOpen || _isSubmitting || _registrationState?.PendingRosterConfirmation is not { } pending)
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.ConfirmTournamentRosterMemberAsync(Tournament.Id, pending.Id),
            "Your roster place is confirmed.");
    }

    private async Task SubmitTeamRosterAsync()
    {
        if(!CanSubmitRoster || _isSubmitting || SelectedTeam is null)
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.SubmitTeamTournamentRosterAsync(Tournament.Id, SelectedTeam.Id, BuildRosterRequest()),
            "The team roster was submitted for this tournament.");
    }

    private async Task UnregisterTeamAsync()
    {
        if(!IsRegistrationOpen || _isSubmitting)
            return;

        var teamId = _registrationState?.ActiveTeamRegistration?.Team?.Id ??
            _registrationState?.CaptainManagedRegistrations
                .Select(registration => registration.Team?.Id)
                .FirstOrDefault(id => id.HasValue);

        if(!teamId.HasValue)
            return;

        await RunRegistrationActionAsync(
            async () =>
            {
                await TournamentService.DeleteTeamTournamentRegistrationAsync(Tournament.Id, teamId.Value);
            },
            "The team registration was removed.");
    }

    private async Task RunRegistrationActionAsync(
        Func<Task> action,
        string successMessage)
    {
        _isSubmitting = true;
        _registrationError = null;
        try
        {
            await action();
            ToastService.ShowSuccess(successMessage);
            await RefreshTournamentAsync();
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _registrationError = "You are not authorized to change this registration.";
            ToastService.ShowError(_registrationError);
        }
        catch(Exception exception)
        {
            _registrationError = GetErrorMessage(exception, "The registration could not be changed right now.");
            ToastService.ShowError(_registrationError);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task RemoveAdminRegistrationAsync(AdminTournamentRegistrationDTO registration)
    {
        if(_pendingAdminRemovalRegistrationId.HasValue)
            return;

        _pendingAdminRemovalRegistrationId = registration.Id;
        _adminError = null;
        try
        {
            if(registration.Kind == TournamentRegistrationKind.Individual && registration.User is not null)
            {
                await TournamentService.RemoveTournamentUserRegistrationAsAdminAsync(
                    Tournament.Id,
                    registration.User.Id,
                    _adminRemovalReason);
            }
            else if(registration.Kind == TournamentRegistrationKind.Team && registration.Team is not null)
            {
                await TournamentService.RemoveTournamentTeamRegistrationAsAdminAsync(
                    Tournament.Id,
                    registration.Team.Id,
                    _adminRemovalReason);
            }
            else
            {
                _adminError = "This registration does not contain enough identity data to remove it.";
                return;
            }

            _adminRemovalReason = string.Empty;
            ToastService.ShowSuccess("The registration was removed.");
            await RefreshTournamentAsync();
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            _adminError = "You are not authorized to remove this registration.";
            ToastService.ShowError(_adminError);
        }
        catch(Exception exception)
        {
            _adminError = GetErrorMessage(exception, "The registration could not be removed right now.");
            ToastService.ShowError(_adminError);
        }
        finally
        {
            _pendingAdminRemovalRegistrationId = null;
        }
    }

    private async Task HandleTeamChangedAsync(ChangeEventArgs args)
    {
        _selectedTeamId = Guid.TryParse(args.Value?.ToString(), out var id) ? id : null;
        await LoadSelectedTeamEligibilityAsync();
    }

    private async Task ToggleRosterMemberAsync(Guid userId, ChangeEventArgs args)
    {
        if(args.Value is bool selected && selected)
            _selectedRosterUserIds.Add(userId);
        else
            _selectedRosterUserIds.Remove(userId);

        await RefreshRosterEligibilityAsync();
    }

    private SubmitTeamRosterDTO BuildRosterRequest() => new()
    {
        TeamId = SelectedTeam?.Id ?? Guid.Empty,
        UserIds = _selectedRosterUserIds.ToArray()
    };

    private async Task RefreshTournamentAsync()
    {
        var updatedTournament = await TournamentService.GetTournamentByIdAsync(Tournament.Id);
        if(updatedTournament is not null)
        {
            Tournament = updatedTournament;
            _participants.Clear();
            _participants.AddRange(BuildParticipants(updatedTournament));
            await OnTournamentUpdated.InvokeAsync(updatedTournament);
        }

        await LoadRegistrationContextAsync();
    }

    private void ResetRegistrationContext()
    {
        _registrationState = null;
        _individualEligibility = null;
        _selectedTeamEligibility = null;
        _rosterEligibility = null;
        _teamSummary = null;
        _selectedTeamId = null;
        _selectedRosterUserIds.Clear();
        _adminRegistrations.Clear();
        _registrationError = null;
        _adminError = null;
        _teamError = null;
    }

    private string GetParticipantModalSummary() =>
        _selectedParticipant?.ParticipationMode == ParticipationMode.Team
            ? "Team roster, captain, and registered members."
            : "Player profile and connected account details.";

    private string GetRegistrationStateMessage()
    {
        if(!IsRegistrationOpen)
            return "Registration is closed because this tournament is no longer scheduled.";

        if(!_isAuthenticated)
            return "Sign in to check eligibility and manage your registration.";

        return Tournament.ParticipationMode == ParticipationMode.Team
            ? "Team captains can submit an eligible roster. Every roster member must confirm when required."
            : "Register yourself while the tournament is scheduled.";
    }

    private static string GetUserLabel(PublicUserDTO? user)
    {
        if(user is null)
            return "Unknown user";

        if(!string.IsNullOrWhiteSpace(user.Username))
            return user.Username.Trim();

        if(!string.IsNullOrWhiteSpace(user.DisplayName))
            return user.DisplayName.Trim();

        var name = string.Join(" ", new[] { user.Firstname, user.Lastname }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));

        return string.IsNullOrWhiteSpace(name) ? "Participant" : name;
    }

    private static string GetReasonText(IEnumerable<string> reasonCodes)
    {
        var reasons = reasonCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Replace('_', ' ').Trim())
            .ToList();

        return reasons.Count == 0 ? "Not eligible for this workflow." : string.Join("; ", reasons);
    }

    private static bool IsUnauthorized(Exception exception) =>
        exception is UnauthorizedAccessException ||
        exception is ApiException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };

    private static string GetErrorMessage(Exception exception, string fallback)
    {
        if(exception is ApiException apiException && !string.IsNullOrWhiteSpace(apiException.Content))
            return apiException.Content!;

        return fallback;
    }
}
