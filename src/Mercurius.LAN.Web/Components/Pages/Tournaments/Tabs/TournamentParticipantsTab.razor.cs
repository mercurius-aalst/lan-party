using System.Net;
using System.Security.Claims;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentParticipantsTab : IDisposable
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<TournamentExtended> OnTournamentUpdated { get; set; }

    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;

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
    private TournamentStatus? _loadedTournamentStatus;
    private ParticipationMode? _loadedParticipationMode;
    private int? _loadedTeamSize;
    private bool _registrationRefreshRequested;
    private CancellationTokenSource? _registrationCancellation;
    private CancellationTokenSource? _rosterEligibilityCancellation;
    private long _registrationLoadGeneration;
    private long _rosterEligibilityGeneration;
    private Guid? _currentUserId;

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
        IsSelectedTeamEligible &&
        IsSelectedRosterEligible;

    private bool HasManagedRegistration =>
        SelectedTeam is not null &&
        _registrationState?.CaptainManagedRegistrations.Any(registration => registration.Team?.Id == SelectedTeam.Id) == true;

    private bool IsSelectedTeamEligible =>
        _selectedTeamEligibility is not null &&
        (_selectedTeamEligibility.Eligible ||
         HasManagedRegistration && IsExistingRegistrationEligibilityUsable(_selectedTeamEligibility));

    private bool IsSelectedRosterEligible =>
        _rosterEligibility is not null &&
        (_rosterEligibility.Eligible ||
         HasManagedRegistration && IsExistingRosterEligibilityUsable(_rosterEligibility, GetManagedRosterUserIds()));

    private bool HasPublicParticipants => _participants.Count > 0;

    private bool HasRegistrationContextError =>
        _registrationError is not null ||
        _teamError is not null ||
        _adminError is not null;

    protected override void OnParametersSet()
    {
        var registrationContextChanged =
            _loadedTournamentId != Tournament.Id ||
            _loadedTournamentStatus != Tournament.Status ||
            _loadedParticipationMode != Tournament.ParticipationMode ||
            _loadedTeamSize != Tournament.TeamSize;

        if(registrationContextChanged)
        {
            _loadedTournamentId = Tournament.Id;
            _loadedTournamentStatus = Tournament.Status;
            _loadedParticipationMode = Tournament.ParticipationMode;
            _loadedTeamSize = Tournament.TeamSize;
            _hasLoadedForTournament = false;
            _registrationRefreshRequested = true;
            CancelRegistrationLoad();
            ResetRegistrationContext();
        }

        UpdateParticipantProjection();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!_registrationRefreshRequested || _hasLoadedForTournament || _isLoadingRegistration || Tournament.Id == Guid.Empty)
            return;

        _registrationRefreshRequested = false;
        await LoadRegistrationContextAsync();
    }

    private void UpdateParticipantProjection()
    {
        TournamentProjectionMapper.PopulateParticipantProjection(Tournament);
        RefreshParticipantCards();
    }

    private void RefreshParticipantCards()
    {
        TeamAssetUrlResolver.Resolve(Configuration, Tournament);
        _participants.Clear();

        if(Tournament.ParticipationMode == ParticipationMode.Team)
        {
            _participants.AddRange(Tournament.Teams.Select(ParticipantViewModel.FromTeam));
            return;
        }

        _participants.AddRange(Tournament.Users.Select(ParticipantViewModel.FromUser));
    }

    private async Task LoadRegistrationContextAsync()
    {
        _registrationCancellation?.Cancel();
        var registrationCancellation = new CancellationTokenSource();
        _registrationCancellation = registrationCancellation;
        var tournamentId = Tournament.Id;
        var loadGeneration = ++_registrationLoadGeneration;

        _isLoadingRegistration = true;
        _registrationError = null;
        _adminError = null;
        _teamError = null;
        var completed = false;

        try
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if(!IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                return;

            var principal = authenticationState.User;
            _isAuthenticated = principal.Identity?.IsAuthenticated == true;
            _isAdmin = _isAuthenticated && principal.IsInRole("admin");
            _currentUserId = GetCurrentUserId(principal);

            if(!_isAuthenticated)
            {
                completed = true;
                return;
            }

            var registrationTask = LoadCurrentRegistrationStateAsync(tournamentId, loadGeneration, registrationCancellation.Token);
            var workflowTask = IsRegistrationOpen
                ? Tournament.ParticipationMode == ParticipationMode.Individual
                    ? LoadIndividualEligibilityAsync(tournamentId, loadGeneration, registrationCancellation.Token)
                    : LoadTeamRegistrationContextAsync(tournamentId, loadGeneration, registrationTask, registrationCancellation.Token)
                : Task.CompletedTask;
            var adminTask = _isAdmin
                ? LoadAdminRegistrationsAsync(tournamentId, loadGeneration, registrationCancellation.Token)
                : Task.CompletedTask;

            await Task.WhenAll(registrationTask, workflowTask, adminTask);
            completed = IsCurrentRegistrationLoad(tournamentId, loadGeneration) &&
                !HasRegistrationContextError;
            if(!completed && _registrationError is null)
                _registrationError = "Some registration options are unavailable right now.";
        }
        catch(OperationCanceledException) when(registrationCancellation.IsCancellationRequested)
        {
            // A newer tournament parameter or registration refresh superseded this request.
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationError = GetErrorMessage(exception, "Registration options are unavailable right now.");
        }
        finally
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
            {
                _registrationCancellation = null;
                _hasLoadedForTournament = completed;
                _isLoadingRegistration = false;
                await InvokeAsync(StateHasChanged);
            }

            registrationCancellation.Dispose();
        }
    }

    private async Task LoadCurrentRegistrationStateAsync(
        Guid tournamentId,
        long loadGeneration,
        CancellationToken cancellationToken)
    {
        try
        {
            var registrationState = await TournamentService.GetCurrentUserTournamentRegistrationStateAsync(
                tournamentId,
                cancellationToken);
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationState = registrationState;
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationError = "Your account is not authorized to view registration status.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationError ??= GetErrorMessage(exception, "Registration status is unavailable right now.");
        }
    }

    private async Task LoadIndividualEligibilityAsync(
        Guid tournamentId,
        long loadGeneration,
        CancellationToken cancellationToken)
    {
        try
        {
            var eligibility = await TournamentService.CheckIndividualTournamentRegistrationEligibilityAsync(
                tournamentId,
                cancellationToken);
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _individualEligibility = eligibility;
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationError ??= "Your account is not authorized to check individual registration eligibility.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _registrationError ??= GetErrorMessage(exception, "Individual registration eligibility is unavailable right now.");
        }
    }

    private async Task LoadTeamRegistrationContextAsync(
        Guid tournamentId,
        long loadGeneration,
        Task registrationTask,
        CancellationToken cancellationToken)
    {
        _isLoadingTeams = true;
        try
        {
            var teamSummary = await TeamService.GetCurrentUserTeamSummaryAsync(cancellationToken);
            await registrationTask;
            if(!IsCurrentRegistrationLoad(tournamentId, loadGeneration) || _registrationError is not null)
                return;

            _teamSummary = teamSummary;
            if(_selectedTeamId.HasValue && CaptainedTeams.All(team => team.Id != _selectedTeamId.Value))
                _selectedTeamId = null;

            if(!_selectedTeamId.HasValue)
                _selectedTeamId = CaptainedTeams.FirstOrDefault()?.Id;

            if(_selectedTeamId.HasValue)
                await LoadSelectedTeamEligibilityAsync(tournamentId, loadGeneration, cancellationToken);
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _teamError = "Sign in with a team captain account to submit a team roster.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _teamError = GetErrorMessage(exception, "Your team registration options are unavailable right now.");
        }
        finally
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _isLoadingTeams = false;
        }
    }

    private async Task LoadSelectedTeamEligibilityAsync(
        Guid tournamentId,
        long loadGeneration,
        CancellationToken cancellationToken)
    {
        CancelRosterEligibilityLoad();
        _selectedTeamEligibility = null;
        _rosterEligibility = null;
        _selectedRosterUserIds.Clear();
        _teamError = null;

        var selectedTeam = SelectedTeam;
        if(selectedTeam is null)
            return;

        var teamEligibilityGeneration = _rosterEligibilityGeneration;

        try
        {
            var selectedTeamEligibility = await TournamentService.CheckTeamTournamentRegistrationEligibilityAsync(
                tournamentId,
                selectedTeam.Id,
                cancellationToken);
            if(!IsCurrentRegistrationLoad(tournamentId, loadGeneration) ||
               teamEligibilityGeneration != _rosterEligibilityGeneration ||
               selectedTeam.Id != SelectedTeam?.Id)
                return;

            _selectedTeamEligibility = selectedTeamEligibility;

            if(_registrationState?.CaptainManagedRegistrations.FirstOrDefault(registration => registration.Team?.Id == selectedTeam.Id) is { } registration)
            {
                foreach(var member in registration.RosterMembers)
                    _selectedRosterUserIds.Add(member.User.Id);
            }
            else
            {
                foreach(var member in selectedTeam.Members)
                    _selectedRosterUserIds.Add(member.Id);
            }

            await RefreshRosterEligibilityAsync(cancellationToken);
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration) &&
               teamEligibilityGeneration == _rosterEligibilityGeneration &&
               selectedTeam.Id == SelectedTeam?.Id)
                _teamError = "You are not authorized to manage this team registration.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration) &&
               teamEligibilityGeneration == _rosterEligibilityGeneration &&
               selectedTeam.Id == SelectedTeam?.Id)
                _teamError = GetErrorMessage(exception, "Team registration eligibility is unavailable right now.");
        }
    }

    private async Task RefreshRosterEligibilityAsync(CancellationToken cancellationToken = default)
    {
        CancelRosterEligibilityLoad();
        var tournamentId = Tournament.Id;
        var selectedTeam = SelectedTeam;
        var selectedUserIds = _selectedRosterUserIds.ToArray();
        _rosterEligibility = null;
        if(selectedTeam is null || selectedUserIds.Length == 0)
            return;

        var eligibilityCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _rosterEligibilityCancellation = eligibilityCancellation;
        var eligibilityGeneration = ++_rosterEligibilityGeneration;
        try
        {
            var eligibility = await TournamentService.CheckTeamRosterEligibilityAsync(
                tournamentId,
                selectedTeam.Id,
                new SubmitTeamRosterDTO
                {
                    TeamId = selectedTeam.Id,
                    UserIds = selectedUserIds
                },
                eligibilityCancellation.Token);
            if(IsCurrentRosterEligibility(tournamentId, selectedTeam.Id, selectedUserIds, eligibilityGeneration))
                _rosterEligibility = eligibility;
        }
        catch(OperationCanceledException) when(eligibilityCancellation.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRosterEligibility(tournamentId, selectedTeam.Id, selectedUserIds, eligibilityGeneration))
                _teamError = "You are not authorized to validate this roster.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRosterEligibility(tournamentId, selectedTeam.Id, selectedUserIds, eligibilityGeneration))
                _teamError = GetErrorMessage(exception, "Roster eligibility is unavailable right now.");
        }
        finally
        {
            if(IsCurrentRosterEligibility(tournamentId, selectedTeam.Id, selectedUserIds, eligibilityGeneration))
                _rosterEligibilityCancellation = null;

            eligibilityCancellation.Dispose();
        }
    }

    private async Task LoadAdminRegistrationsAsync(
        Guid tournamentId,
        long loadGeneration,
        CancellationToken cancellationToken)
    {
        _isLoadingAdmin = true;
        try
        {
            var registrations = await TournamentService.GetAdminTournamentRegistrationsAsync(
                tournamentId,
                1,
                100,
                cancellationToken);
            if(!IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                return;

            _adminRegistrations.Clear();
            _adminRegistrations.AddRange(registrations);
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _adminError = "You are not authorized to view the registration administration list.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
                _adminError = GetErrorMessage(exception, "The administration registration list is unavailable right now.");
        }
        finally
        {
            if(IsCurrentRegistrationLoad(tournamentId, loadGeneration))
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

        var registrationId = _registrationState?.IndividualRegistration?.Id;
        if(!registrationId.HasValue)
            return;

        await RunRegistrationActionAsync(
            async () =>
            {
                await TournamentService.DeleteCurrentUserTournamentRegistrationAsync(Tournament.Id);
            },
            "Your tournament registration was removed.",
            registrationId);
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
                .FirstOrDefault(registration => registration.Team is not null)
                ?.Team?.Id;

        var managedRegistration = _registrationState?.CaptainManagedRegistrations
            .FirstOrDefault(registration => registration.Team?.Id == teamId);

        if(!teamId.HasValue || managedRegistration is null)
            return;

        await RunRegistrationActionAsync(
            async () =>
            {
                await TournamentService.DeleteTeamTournamentRegistrationAsync(Tournament.Id, teamId.Value);
            },
            "The team registration was removed.",
            managedRegistration.Id);
    }

    private async Task RunRegistrationActionAsync(
        Func<Task<TournamentRegistrationDTO>> action,
        string successMessage,
        Guid? removedRegistrationId = null)
    {
        await RunRegistrationActionCoreAsync(
            async () => await action(),
            successMessage,
            removedRegistrationId);
    }

    private async Task RunRegistrationActionAsync(
        Func<Task> action,
        string successMessage,
        Guid? removedRegistrationId = null)
    {
        await RunRegistrationActionCoreAsync(
            async () =>
            {
                await action();
                return null;
            },
            successMessage,
            removedRegistrationId);
    }

    private async Task RunRegistrationActionCoreAsync(
        Func<Task<TournamentRegistrationDTO?>> action,
        string successMessage,
        Guid? removedRegistrationId)
    {
        _isSubmitting = true;
        _registrationError = null;
        try
        {
            var registration = await action();
            if(registration is not null)
                ApplyRegistrationMutation(registration);
            else if(removedRegistrationId.HasValue)
                ApplyRegistrationRemoval(removedRegistrationId.Value);

            ToastService.ShowSuccess(successMessage);
            await OnTournamentUpdated.InvokeAsync(Tournament);
            await InvokeAsync(StateHasChanged);
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

            ApplyRegistrationRemoval(registration.Id);
            _adminRegistrations.RemoveAll(candidate => candidate.Id == registration.Id);
            _adminRemovalReason = string.Empty;
            ToastService.ShowSuccess("The registration was removed.");
            await OnTournamentUpdated.InvokeAsync(Tournament);
            await InvokeAsync(StateHasChanged);
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
        await LoadSelectedTeamEligibilityAsync(Tournament.Id, _registrationLoadGeneration, CancellationToken.None);
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

    private async Task RetryRegistrationContextAsync()
    {
        _hasLoadedForTournament = false;
        await LoadRegistrationContextAsync();
    }

    private void ApplyRegistrationMutation(TournamentRegistrationDTO registration)
    {
        TournamentProjectionMapper.ApplyRegistration(Tournament, registration);
        RefreshParticipantCards();

        if(_registrationState is null)
            return;

        var currentUserId = ResolveCurrentUserId(registration);
        var individualRegistration = _registrationState.IndividualRegistration;
        var activeTeamRegistration = _registrationState.ActiveTeamRegistration;
        var pendingRosterConfirmation = _registrationState.PendingRosterConfirmation;
        var captainManagedRegistrations = _registrationState.CaptainManagedRegistrations.ToList();

        if(registration.Kind == TournamentRegistrationKind.Individual)
        {
            individualRegistration = registration;
        }
        else
        {
            captainManagedRegistrations.RemoveAll(existing =>
                existing.Id == registration.Id ||
                existing.Team?.Id == registration.Team?.Id);

            if(registration.Team?.CaptainUserId == currentUserId ||
               _registrationState.CaptainManagedRegistrations.Any(existing => existing.Team?.Id == registration.Team?.Id))
            {
                captainManagedRegistrations.Add(registration);
            }

            pendingRosterConfirmation = registration.RosterMembers.FirstOrDefault(member =>
                member.User.Id == currentUserId &&
                member.ConfirmationStatus == RosterMemberConfirmationStatus.Pending);
            activeTeamRegistration = registration.Status == TournamentRegistrationStatus.Active &&
                registration.RosterMembers.Any(member => member.User.Id == currentUserId)
                    ? registration
                    : null;

            if(SelectedTeam?.Id == registration.Team?.Id)
            {
                _selectedRosterUserIds.Clear();
                foreach(var member in registration.RosterMembers)
                    _selectedRosterUserIds.Add(member.User.Id);

                _selectedTeamEligibility = new EligibilityResponseDTO { Eligible = true };
                _rosterEligibility = new RosterCandidateEligibilityResponseDTO { Eligible = true };
            }
        }

        _registrationState = new CurrentUserTournamentRegistrationStateDTO
        {
            TournamentId = _registrationState.TournamentId,
            IndividualRegistration = individualRegistration,
            PendingRosterConfirmation = pendingRosterConfirmation,
            ActiveTeamRegistration = activeTeamRegistration,
            CaptainManagedRegistrations = captainManagedRegistrations,
            CanRegisterIndividual = false,
            CanConfirmRoster = pendingRosterConfirmation is not null,
            CanUnregister = individualRegistration is not null ||
                activeTeamRegistration is not null ||
                captainManagedRegistrations.Count > 0
        };
    }

    private void ApplyRegistrationRemoval(Guid registrationId)
    {
        TournamentProjectionMapper.RemoveRegistration(Tournament, registrationId);
        RefreshParticipantCards();

        if(_registrationState is null)
            return;

        var removedTeamRegistration = _registrationState.ActiveTeamRegistration?.Id == registrationId ||
            _registrationState.CaptainManagedRegistrations.Any(registration => registration.Id == registrationId);
        var removedIndividualRegistration = _registrationState.IndividualRegistration?.Id == registrationId;
        var individualRegistration = removedIndividualRegistration
            ? null
            : _registrationState.IndividualRegistration;
        var activeTeamRegistration = _registrationState.ActiveTeamRegistration?.Id == registrationId
            ? null
            : _registrationState.ActiveTeamRegistration;
        var captainManagedRegistrations = _registrationState.CaptainManagedRegistrations
            .Where(registration => registration.Id != registrationId)
            .ToList();

        if(removedTeamRegistration)
        {
            activeTeamRegistration = null;
            _selectedTeamEligibility = new EligibilityResponseDTO { Eligible = true };
            _rosterEligibility = null;
        }

        if(removedIndividualRegistration)
            _individualEligibility = new EligibilityResponseDTO { Eligible = true };

        _registrationState = new CurrentUserTournamentRegistrationStateDTO
        {
            TournamentId = _registrationState.TournamentId,
            IndividualRegistration = individualRegistration,
            PendingRosterConfirmation = removedTeamRegistration
                ? null
                : _registrationState.PendingRosterConfirmation,
            ActiveTeamRegistration = activeTeamRegistration,
            CaptainManagedRegistrations = captainManagedRegistrations,
            CanRegisterIndividual = Tournament.ParticipationMode == ParticipationMode.Individual &&
                individualRegistration is null &&
                activeTeamRegistration is null &&
                _registrationState.PendingRosterConfirmation is null,
            CanConfirmRoster = !removedTeamRegistration && _registrationState.PendingRosterConfirmation is not null,
            CanUnregister = individualRegistration is not null ||
                activeTeamRegistration is not null ||
                captainManagedRegistrations.Count > 0
        };
    }

    private Guid? ResolveCurrentUserId(TournamentRegistrationDTO registration) =>
        _currentUserId ??
        _registrationState?.PendingRosterConfirmation?.User.Id ??
        _registrationState?.IndividualRegistration?.User?.Id ??
        (registration.Kind == TournamentRegistrationKind.Individual ? registration.User?.Id : registration.Team?.CaptainUserId);

    private HashSet<Guid> GetManagedRosterUserIds()
    {
        var managedRegistration = _registrationState?.CaptainManagedRegistrations
            .FirstOrDefault(registration => registration.Team?.Id == SelectedTeam?.Id);
        return managedRegistration?.RosterMembers.Select(member => member.User.Id).ToHashSet() ?? [];
    }

    internal static bool IsExistingRegistrationEligibilityUsable(EligibilityResponseDTO eligibility) =>
        eligibility.Eligible || AreExistingRegistrationReasonsAllowed(eligibility.ReasonCodes);

    internal static bool IsExistingRosterEligibilityUsable(
        RosterCandidateEligibilityResponseDTO eligibility,
        IReadOnlySet<Guid> existingRosterUserIds)
    {
        if(eligibility.Eligible)
            return true;

        if(!AreExistingRegistrationReasonsAllowed(eligibility.ReasonCodes))
            return false;

        return eligibility.Candidates.All(candidate =>
            candidate.Eligible ||
            existingRosterUserIds.Contains(candidate.UserId) &&
            AreExistingRegistrationReasonsAllowed(candidate.ReasonCodes));
    }

    private static bool AreExistingRegistrationReasonsAllowed(IEnumerable<string> reasonCodes)
    {
        var reasons = reasonCodes.ToList();
        return reasons.Count > 0 && reasons.All(IsExistingRegistrationConflictReason);
    }

    private static bool IsExistingRegistrationConflictReason(string reasonCode) =>
        reasonCode is "team_already_registered" or
            "captain_duplicate_participation" or
            "duplicate_participation" or
            "roster_candidate_ineligible";

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
        _isAuthenticated = false;
        _isAdmin = false;
        _currentUserId = null;
        _isLoadingTeams = false;
        _isLoadingAdmin = false;
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

    private bool IsCurrentRegistrationLoad(Guid tournamentId, long loadGeneration) =>
        tournamentId == Tournament.Id &&
        loadGeneration == _registrationLoadGeneration;

    private bool IsCurrentRosterEligibility(
        Guid tournamentId,
        Guid teamId,
        IReadOnlyCollection<Guid> userIds,
        long eligibilityGeneration) =>
        tournamentId == Tournament.Id &&
        teamId == SelectedTeam?.Id &&
        eligibilityGeneration == _rosterEligibilityGeneration &&
        userIds.Count == _selectedRosterUserIds.Count &&
        userIds.All(_selectedRosterUserIds.Contains);

    private void CancelRegistrationLoad()
    {
        ++_registrationLoadGeneration;
        var cancellation = _registrationCancellation;
        _registrationCancellation = null;
        cancellation?.Cancel();
    }

    private void CancelRosterEligibilityLoad()
    {
        ++_rosterEligibilityGeneration;
        var cancellation = _rosterEligibilityCancellation;
        _rosterEligibilityCancellation = null;
        cancellation?.Cancel();
    }

    private static Guid? GetCurrentUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    private static string GetErrorMessage(Exception exception, string fallback)
    {
        if(exception is ApiException apiException && !string.IsNullOrWhiteSpace(apiException.Content))
            return apiException.Content!;

        return fallback;
    }

    public void Dispose()
    {
        CancelRegistrationLoad();
        CancelRosterEligibilityLoad();
    }
}
