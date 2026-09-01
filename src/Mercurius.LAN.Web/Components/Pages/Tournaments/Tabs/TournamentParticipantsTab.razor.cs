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
using MudBlazor;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentParticipantsTab : IDisposable
{
    // The backend roster-eligibility endpoint rejects requests with more than 50 user ids.
    private const int MaxRosterEligibilityUserIds = 50;

    private static readonly HashSet<string> ExistingRegistrationConflictCodes =
    [
        "team_already_registered",
        "captain_duplicate_participation",
        "duplicate_participation",
        "roster_candidate_ineligible"
    ];

    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<TournamentExtended> OnTournamentUpdated { get; set; }

    [Inject] private ITeamService TeamService { get; set; } = null!;
    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ITeamRealtimeService TeamRealtimeService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    private readonly List<ParticipantViewModel> _participants = [];
    private readonly List<AdminTournamentRegistrationDTO> _adminRegistrations = [];
    private readonly HashSet<Guid> _selectedRosterUserIds = [];
    private readonly Dictionary<Guid, EligibilityResponseDTO> _teamEligibilityById = [];
    private readonly Dictionary<Guid, RosterCandidateEligibilityDTO> _rosterCandidatesById = [];

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
    private bool _teamSummaryUnavailable;
    private bool _teamEligibilityUnavailable;
    private bool _rosterEligibilityUnavailable;
    private bool _isAuthenticated;
    private bool _isAdmin;
    private bool _isLoadingRegistration;
    private bool _isLoadingTeams;
    private bool _isLoadingRoster;
    private bool _isLoadingAdmin;
    private bool _isSubmitting;
    private bool _hasLoadedForTournament;
    private bool _isDisposed;
    private bool _hasDirtyRosterDraft;
    private readonly TeamStateInvalidationGate _teamStateInvalidationGate = new();
    private int _activeTeamStep;
    private long _requestGeneration;
    private Guid _loadedTournamentId;
    private string _loadedRegistrationFingerprint = string.Empty;
    private string? _registrationWarning;
    private MudStepper? _teamRegistrationStepper;

    private IReadOnlyList<TeamManagementSummaryDTO> CaptainedTeams =>
        _teamSummary?.CaptainedTeams ?? [];

    private TeamManagementSummaryDTO? SelectedTeam =>
        _selectedTeamId.HasValue
            ? CaptainedTeams.FirstOrDefault(team => team.Id == _selectedTeamId.Value)
            : null;

    private IReadOnlyList<PublicUserDTO> EditableRosterCandidates
    {
        get
        {
            var candidates = (SelectedTeam?.Members ?? []).ToList();
            var candidateIds = candidates.Select(member => member.Id).ToHashSet();
            foreach(var member in CaptainManagedRegistration?.RosterMembers ?? [])
            {
                if(member.User is not null && candidateIds.Add(member.User.Id))
                    candidates.Add(member.User);
            }

            foreach(var candidateId in MergeEditableRosterCandidateIds(
                        candidates.Select(member => member.Id),
                        _selectedRosterUserIds,
                        _rosterCandidatesById.Keys))
            {
                if(candidateIds.Add(candidateId))
                {
                    var candidateUser = _rosterCandidatesById.TryGetValue(candidateId, out var eligibilityCandidate)
                        ? eligibilityCandidate.User
                        : null;
                    candidates.Add(candidateUser ?? new PublicUserDTO
                    {
                        Id = candidateId,
                        DisplayName = "Former roster member"
                    });
                }
            }

            if(SelectedTeam is { } team && team.CaptainUserId != Guid.Empty && candidateIds.Add(team.CaptainUserId))
            {
                candidates.Add(new PublicUserDTO
                {
                    Id = team.CaptainUserId,
                    Username = team.CaptainUsername,
                    DisplayName = string.IsNullOrWhiteSpace(team.CaptainUsername)
                        ? "Current captain"
                        : team.CaptainUsername
                });
            }

            return candidates;
        }
    }

    private TournamentRegistrationDTO? CaptainManagedRegistration =>
        SelectedTeam is null
            ? null
            : (_registrationState?.CaptainManagedRegistrations ?? [])
                .FirstOrDefault(registration => registration.Team?.Id == SelectedTeam.Id);

    private TournamentRegistrationDTO? CurrentTeamRegistration =>
        _registrationState?.CurrentTeamRegistration ?? _registrationState?.ActiveTeamRegistration;

    private bool HasCaptainManagedRegistration => CaptainManagedRegistration is not null;

    private int RequiredTeamSize => Tournament.TeamSize.GetValueOrDefault();

    private bool IsRegistrationOpen => Tournament.Status == TournamentStatus.Scheduled;

    private bool CanAdvanceFromTeamSelection =>
        SelectedTeam is not null &&
        _selectedTeamEligibility is not null &&
        CanUseTeamEligibility(SelectedTeam.Id, _selectedTeamEligibility);

    private bool HasLocalRosterShape =>
        SelectedTeam is not null &&
        RequiredTeamSize > 0 &&
        _selectedRosterUserIds.Count == RequiredTeamSize &&
        _selectedRosterUserIds.Contains(SelectedTeam.CaptainUserId);

    private bool IsRosterEligibleForWorkflow =>
        _rosterEligibility is not null &&
        (_rosterEligibility.Eligible ||
         (HasCaptainManagedRegistration &&
          _rosterEligibility.ReasonCodes.Count > 0 &&
          _rosterEligibility.ReasonCodes.All(IsExistingRegistrationConflictCode) &&
          _rosterEligibility.Candidates.All(candidate =>
              candidate.Eligible || IsExistingRosterConflictAllowed(candidate.UserId, candidate))));

    private bool CanSubmitRoster =>
        IsRegistrationOpen &&
        !_isLoadingRegistration &&
        !_isLoadingRoster &&
        !_rosterEligibilityUnavailable &&
        SelectedTeam is not null &&
        CanAdvanceFromTeamSelection &&
        HasLocalRosterShape &&
        IsRosterEligibleForWorkflow;

    private bool CanAdvanceTeamStep => _activeTeamStep switch
    {
        0 => CanAdvanceFromTeamSelection,
        1 => HasLocalRosterShape && IsRosterEligibleForWorkflow,
        _ => false
    };

    private bool CanConfirmPending =>
        IsRegistrationOpen &&
        !_isLoadingRegistration &&
        _registrationState?.CanConfirmRoster == true &&
        _registrationState.PendingRosterConfirmation is not null;

    private bool CanUnregisterSelectedTeam =>
        IsRegistrationOpen &&
        !_isLoadingRegistration &&
        !_isLoadingRoster &&
        !_isSubmitting &&
        _registrationState?.CanUnregister == true &&
        CaptainManagedRegistration is not null;

    private bool HasPublicParticipants => _participants.Count > 0;

    private bool HasDirtyRosterDraft => _hasDirtyRosterDraft;

    private bool IsCurrentTeamMember(Guid userId) =>
        (SelectedTeam?.Members ?? []).Any(member => member.Id == userId);

    private string LoginUrl =>
        $"/account/login?returnUrl={Uri.EscapeDataString($"/tournaments/{Tournament.Id}#tournament-participants")}";

    protected override void OnInitialized()
    {
        TeamRealtimeService.TeamStateInvalidated += HandleTeamStateInvalidatedAsync;
    }

    protected override void OnParametersSet()
    {
        var registrationFingerprint = GetRegistrationFingerprint(Tournament);
        if(_loadedTournamentId != Tournament.Id || _loadedRegistrationFingerprint != registrationFingerprint)
        {
            var discardedDraft = _hasDirtyRosterDraft;
            _loadedTournamentId = Tournament.Id;
            _loadedRegistrationFingerprint = registrationFingerprint;
            _requestGeneration++;
            _hasLoadedForTournament = false;
            ResetRegistrationContext();
            if(discardedDraft)
                _registrationWarning = "Tournament registration settings changed, so your unsaved roster draft was cleared. Review the current roster before saving.";
        }

        _participants.Clear();
        _participants.AddRange(BuildParticipants(Tournament));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_hasLoadedForTournament || Tournament.Id == Guid.Empty)
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
                    Members = (registration.RosterMembers ?? [])
                        .Where(member => member.User is not null)
                        .Select(member => member.User)
                        .ToList()
                });
            }
        }
    }

    private static string GetRegistrationFingerprint(TournamentExtended tournament) =>
        $"{tournament.Id:N}|{tournament.ParticipationMode}|{tournament.TeamSize?.ToString() ?? "none"}|{tournament.Status}|{tournament.PlannedStartTime.Ticks}|{tournament.StartTime.Ticks}|{tournament.EndTime.Ticks}";

    private async Task<bool> LoadRegistrationContextAsync(
        bool preserveRosterDraft = false,
        bool preserveSubmitting = false,
        long? requestGenerationOverride = null)
    {
        var currentUserStateLoaded = false;
        var tournamentId = Tournament.Id;
        var generation = requestGenerationOverride ?? ++_requestGeneration;
        var draftTeamId = preserveRosterDraft && _hasDirtyRosterDraft ? _selectedTeamId : null;
        var draftRosterUserIds = draftTeamId.HasValue ? _selectedRosterUserIds.ToHashSet() : null;
        var draftStep = draftTeamId.HasValue ? _activeTeamStep : 0;
        if(!draftTeamId.HasValue)
            _hasDirtyRosterDraft = false;

        _isLoadingRegistration = true;
        _isLoadingTeams = false;
        _isLoadingRoster = false;
        _isLoadingAdmin = false;
        _isSubmitting = preserveSubmitting;
        if(!preserveSubmitting)
            _pendingAdminRemovalRegistrationId = null;
        _registrationError = null;
        _adminError = null;
        _teamError = null;
        _teamSummaryUnavailable = false;
        _teamEligibilityUnavailable = false;
        _rosterEligibilityUnavailable = false;

        if(!_isDisposed)
            await InvokeAsync(StateHasChanged);

        try
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if(!IsCurrentRequest(tournamentId, generation))
                return currentUserStateLoaded;

            var principal = authenticationState.User;
            _isAuthenticated = principal.Identity?.IsAuthenticated == true;
            _isAdmin = _isAuthenticated && principal.IsInRole("admin");

            if(!_isAuthenticated)
            {
                _registrationState = null;
                _adminRegistrations.Clear();
                _individualEligibility = null;
                return true;
            }

            try
            {
                var state = await TournamentService.GetCurrentUserTournamentRegistrationStateAsync(tournamentId);
                currentUserStateLoaded = true;
                if(IsCurrentRequest(tournamentId, generation))
                    _registrationState = state;
            }
            catch(Exception exception) when(IsUnauthorized(exception))
            {
                if(IsCurrentRequest(tournamentId, generation))
                    _registrationError = "Your account is not authorized to view registration status.";
            }
            catch(Exception exception)
            {
                if(IsCurrentRequest(tournamentId, generation))
                    _registrationError = GetErrorMessage(exception, "Registration status is unavailable right now.");
            }

            if(!IsCurrentRequest(tournamentId, generation))
                return currentUserStateLoaded;

            if(IsRegistrationOpen)
            {
                if(Tournament.ParticipationMode == ParticipationMode.Individual)
                    await LoadIndividualEligibilityAsync(tournamentId, generation);
                else
                    await LoadTeamRegistrationContextAsync(
                        tournamentId,
                        generation,
                        draftTeamId,
                        draftRosterUserIds,
                        draftStep);
            }

            if(_isAdmin && IsCurrentRequest(tournamentId, generation))
                await LoadAdminRegistrationsAsync(tournamentId, generation);
        }
        catch(OperationCanceledException)
        {
            // A newer tournament route or eligibility request owns the state now.
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
                _registrationError = GetErrorMessage(exception, "Registration options are unavailable right now.");
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                if(draftTeamId.HasValue && _selectedTeamId == draftTeamId)
                {
                    _hasDirtyRosterDraft = true;
                    _teamError ??= "A live registration update was received; your unsaved roster draft was kept. Review it before saving.";
                }
                else if(draftTeamId.HasValue)
                {
                    _hasDirtyRosterDraft = false;
                    if(SelectedTeam is null)
                        _selectedRosterUserIds.Clear();
                    _activeTeamStep = 0;
                    _registrationWarning =
                        "The team from your unsaved roster draft is no longer available, so the draft was cleared. Select an available team and build the roster again.";
                }
                _isLoadingRegistration = false;
                if(preserveSubmitting)
                {
                    _isSubmitting = false;
                    _pendingAdminRemovalRegistrationId = null;
                }
                await InvokeAsync(StateHasChanged);
            }
        }

        return currentUserStateLoaded;
    }

    private async Task LoadIndividualEligibilityAsync(Guid tournamentId, long generation)
    {
        try
        {
            var eligibility = await TournamentService.CheckIndividualTournamentRegistrationEligibilityAsync(tournamentId);
            if(IsCurrentRequest(tournamentId, generation))
                _individualEligibility = eligibility;
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, generation))
                _registrationError ??= "Your account is not authorized to check individual registration eligibility.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
                _registrationError ??= GetErrorMessage(exception, "Individual registration eligibility is unavailable right now.");
        }
    }

    private async Task LoadTeamRegistrationContextAsync(
        Guid tournamentId,
        long generation,
        Guid? draftTeamId,
        HashSet<Guid>? draftRosterUserIds,
        int draftStep)
    {
        if(!IsCurrentRequest(tournamentId, generation))
            return;

        _isLoadingTeams = true;
        _teamSummaryUnavailable = false;
        _teamEligibilityUnavailable = false;
        _teamEligibilityById.Clear();
        _selectedTeamEligibility = null;
        _rosterEligibility = null;
        _rosterCandidatesById.Clear();
        await InvokeAsync(StateHasChanged);
        try
        {
            var teamSummary = await TeamService.GetCurrentUserTeamSummaryAsync();
            if(!IsCurrentRequest(tournamentId, generation))
                return;

            _teamSummary = teamSummary;
            try
            {
                await TeamRealtimeService.StartAsync();
                await TeamRealtimeService.JoinTeamsAsync(
                    CaptainedTeams
                        .Concat(_teamSummary.MemberTeams)
                        .Select(team => team.Id));
            }
            catch(Exception)
            {
                // Registration remains usable through the explicit refresh and mutation paths.
                if(IsCurrentRequest(tournamentId, generation))
                    _teamError ??= "Live team updates are unavailable; registration state still refreshes after actions.";
            }

            if(!IsCurrentRequest(tournamentId, generation))
                return;

            var managedTeamId = (_registrationState?.CaptainManagedRegistrations ?? [])
                .Select(registration => registration.Team?.Id)
                .FirstOrDefault(id => id.HasValue && CaptainedTeams.Any(team => team.Id == id.Value));

            if(draftTeamId.HasValue && CaptainedTeams.Any(team => team.Id == draftTeamId.Value))
                _selectedTeamId = draftTeamId;
            else if(managedTeamId.HasValue)
                _selectedTeamId = managedTeamId;
            else if(_selectedTeamId.HasValue && CaptainedTeams.All(team => team.Id != _selectedTeamId.Value))
                _selectedTeamId = null;

            _selectedTeamId ??= CaptainedTeams
                .Where(team =>
                    RequiredTeamSize > 0 &&
                    team.Members.Count >= RequiredTeamSize &&
                    team.Members.Any(member => member.Id == team.CaptainUserId))
                .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault()?.Id
                ?? CaptainedTeams.FirstOrDefault()?.Id;

            if(_selectedTeamId.HasValue)
                await LoadSelectedTeamEligibilityAsync(
                    tournamentId,
                    generation,
                    draftTeamId == _selectedTeamId,
                    draftRosterUserIds,
                    draftStep);
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _teamSummaryUnavailable = true;
                _teamError = "Sign in with a team captain account to submit a team roster.";
            }
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _teamSummaryUnavailable = true;
                _teamError = GetErrorMessage(exception, "Your team registration options are unavailable right now.");
            }
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, generation))
                _isLoadingTeams = false;
        }
    }

    private async Task LoadSelectedTeamEligibilityAsync(
        Guid tournamentId,
        long generation,
        bool preserveRosterDraft = false,
        HashSet<Guid>? draftRosterUserIds = null,
        int draftStep = 0)
    {
        if(!IsCurrentRequest(tournamentId, generation))
            return;

        var keepDraft = preserveRosterDraft && draftRosterUserIds is not null;
        _selectedTeamEligibility = null;
        _teamEligibilityUnavailable = false;
        _rosterEligibility = null;
        _rosterEligibilityUnavailable = false;
        _rosterCandidatesById.Clear();
        if(keepDraft)
        {
            _selectedRosterUserIds.Clear();
            _selectedRosterUserIds.UnionWith(draftRosterUserIds!);
            _activeTeamStep = Math.Clamp(draftStep, 0, 2);
        }
        else
        {
            _selectedRosterUserIds.Clear();
            _activeTeamStep = 0;
        }

        if(SelectedTeam is null)
            return;

        try
        {
            _teamError = null;
            if(!_teamEligibilityById.TryGetValue(SelectedTeam.Id, out var teamEligibility))
            {
                teamEligibility = await TournamentService.CheckTeamTournamentRegistrationEligibilityAsync(
                    tournamentId,
                    SelectedTeam.Id);
                if(IsCurrentRequest(tournamentId, generation))
                    _teamEligibilityById[SelectedTeam.Id] = teamEligibility;
            }

            if(!IsCurrentRequest(tournamentId, generation))
                return;

            _selectedTeamEligibility = teamEligibility;
            if(!keepDraft && CaptainManagedRegistration is { } registration)
            {
                var savedRosterMembers = registration.RosterMembers ?? [];
                foreach(var member in savedRosterMembers)
                {
                    if(member.User is not null)
                        _selectedRosterUserIds.Add(member.User.Id);
                }
            }
            else if(!keepDraft)
            {
                var teamMembers = SelectedTeam.Members ?? [];
                if(teamMembers.Any(member => member.Id == SelectedTeam.CaptainUserId))
                    _selectedRosterUserIds.Add(SelectedTeam.CaptainUserId);

                var remainingSlots = Math.Max(RequiredTeamSize - _selectedRosterUserIds.Count, 0);
                foreach(var member in teamMembers.Where(member => member.Id != SelectedTeam.CaptainUserId).Take(remainingSlots))
                    _selectedRosterUserIds.Add(member.Id);
            }

            if((keepDraft || CaptainManagedRegistration is not null) && SelectedTeam.CaptainUserId != Guid.Empty)
            {
                var selectedBeforeCaptainReconciliation = _selectedRosterUserIds.ToHashSet();
                var reconciledRoster = ReconcileRosterForCurrentCaptain(
                    _selectedRosterUserIds,
                    SelectedTeam.CaptainUserId);

                _selectedRosterUserIds.Clear();
                _selectedRosterUserIds.UnionWith(reconciledRoster);
                if(!selectedBeforeCaptainReconciliation.SetEquals(_selectedRosterUserIds))
                    _registrationWarning = GetCaptainTransferWarning(
                        _selectedRosterUserIds.Count,
                        RequiredTeamSize);
            }

            await RefreshRosterEligibilityAsync(tournamentId, generation, includeCandidateReasons: true);
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _teamEligibilityUnavailable = true;
                _teamError = "You are not authorized to manage this team registration.";
            }
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _teamEligibilityUnavailable = true;
                _teamError = GetErrorMessage(exception, "Team registration eligibility is unavailable right now.");
            }
        }
    }

    private async Task RefreshRosterEligibilityAsync(
        Guid tournamentId,
        long generation,
        bool includeCandidateReasons = false)
    {
        if(!IsCurrentRequest(tournamentId, generation))
            return;

        _teamError = null;
        _rosterEligibility = null;
        if(SelectedTeam is null)
            return;

        _isLoadingRoster = true;
        _rosterEligibilityUnavailable = false;
        await InvokeAsync(StateHasChanged);
        try
        {
            var selectedUserIds = _selectedRosterUserIds.ToArray();
            var eligibility = await TournamentService.CheckTeamRosterEligibilityAsync(
                tournamentId,
                SelectedTeam.Id,
                new SubmitTeamRosterDTO
                {
                    TeamId = SelectedTeam.Id,
                    UserIds = selectedUserIds
                });

            if(!IsCurrentRequest(tournamentId, generation))
                return;

            _rosterEligibility = eligibility;
            MergeRosterCandidateEligibility(eligibility.Candidates);

            if(includeCandidateReasons)
            {
                var candidateUserIds = EditableRosterCandidates
                    .Select(member => member.Id)
                    .Distinct()
                    .ToArray();
                if(!candidateUserIds.ToHashSet().SetEquals(selectedUserIds))
                {
                    foreach(var candidateUserIdBatch in BatchRosterEligibilityUserIds(candidateUserIds))
                    {
                        var candidateEligibility = await TournamentService.CheckTeamRosterEligibilityAsync(
                            tournamentId,
                            SelectedTeam.Id,
                            new SubmitTeamRosterDTO
                            {
                                TeamId = SelectedTeam.Id,
                                UserIds = candidateUserIdBatch
                            });
                        if(!IsCurrentRequest(tournamentId, generation))
                            return;

                        MergeRosterCandidateEligibility(candidateEligibility.Candidates);
                    }
                }
            }
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _rosterEligibilityUnavailable = true;
                _teamError = "You are not authorized to validate this roster.";
            }
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
            {
                _rosterEligibilityUnavailable = true;
                _teamError = GetErrorMessage(exception, "Roster eligibility is unavailable right now.");
            }
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, generation))
                _isLoadingRoster = false;
        }
    }

    private async Task LoadAdminRegistrationsAsync(Guid tournamentId, long generation)
    {
        _isLoadingAdmin = true;
        try
        {
            var registrations = await TournamentService.GetAdminTournamentRegistrationsAsync(tournamentId, 1, 100);
            if(IsCurrentRequest(tournamentId, generation))
            {
                _adminRegistrations.Clear();
                _adminRegistrations.AddRange(registrations);
            }
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, generation))
                _adminError = "You are not authorized to view the registration administration list.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, generation))
                _adminError = GetErrorMessage(exception, "The administration registration list is unavailable right now.");
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, generation))
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
           _isLoadingRegistration ||
           _registrationState?.IndividualRegistration is not null ||
           _registrationState?.CanRegisterIndividual != true ||
           _individualEligibility?.Eligible != true)
            return;

        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;

        if(!await ConfirmMutationAsync(
               "Confirm individual registration",
               $"Register your account for {Tournament.Name}?",
               "Register",
               requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        if(!await RevalidateIndividualRegistrationAsync(registering: true, requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.RegisterCurrentUserForTournamentAsync(tournamentId),
            "You are registered for this tournament.",
            tournamentId,
            requestGeneration);
    }

    private async Task UnregisterIndividualAsync()
    {
        if(!IsRegistrationOpen ||
           _isSubmitting ||
           _isLoadingRegistration ||
           _registrationState?.IndividualRegistration is null ||
           _registrationState.CanUnregister != true)
            return;

        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;

        if(!await ConfirmMutationAsync(
               "Confirm individual unregister",
               $"Remove your registration from {Tournament.Name}?",
               "Unregister",
               requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        if(!await RevalidateIndividualRegistrationAsync(registering: false, requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.DeleteCurrentUserTournamentRegistrationAsync(tournamentId),
            "Your tournament registration was removed.",
            tournamentId,
            requestGeneration);
    }

    private async Task ConfirmRosterMemberAsync()
    {
        if(!CanConfirmPending || _isSubmitting)
            return;

        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;

        if(!await ReloadCurrentUserStateAsync(requestGeneration))
            return;

        var pending = _registrationState?.PendingRosterConfirmation;
        if(!CanConfirmPending || pending is null)
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.ConfirmTournamentRosterMemberAsync(tournamentId, pending.Id),
            "Your roster place is confirmed.",
            tournamentId,
            requestGeneration);
    }

    private async Task SubmitTeamRosterAsync()
    {
        if(!CanSubmitRoster || _isSubmitting || SelectedTeam is null)
            return;

        var teamId = SelectedTeam.Id;
        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;
        var request = BuildRosterRequest();
        if(!await RevalidateRosterBeforeSubmitAsync(teamId, request, requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration) || !CanSubmitRoster || SelectedTeam?.Id != teamId)
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.SubmitTeamTournamentRosterAsync(tournamentId, teamId, request),
            HasCaptainManagedRegistration
                ? "The team roster changes were saved."
                : "The team roster was submitted for this tournament.",
            tournamentId,
            requestGeneration);
    }

    private async Task UnregisterTeamAsync()
    {
        if(!CanUnregisterSelectedTeam || SelectedTeam is null)
            return;

        var teamId = SelectedTeam.Id;
        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;
        if(!await ConfirmMutationAsync(
               "Confirm team unregister",
               $"Remove {SelectedTeam.Name} from {Tournament.Name}? Pending roster confirmations will be removed too.",
               "Unregister team",
               requestGeneration))
            return;

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        if(!await ReloadCurrentUserStateAsync(requestGeneration))
            return;

        var latestRegistration = (_registrationState?.CaptainManagedRegistrations ?? [])
            .FirstOrDefault(registration => registration.Team?.Id == teamId);
        if(!IsCurrentRequest(tournamentId, requestGeneration) ||
           !IsRegistrationOpen ||
           _registrationState?.CanUnregister != true ||
           latestRegistration is null)
            return;

        await RunRegistrationActionAsync(
            () => TournamentService.DeleteTeamTournamentRegistrationAsync(tournamentId, teamId),
            "The team registration was removed.",
            tournamentId,
            requestGeneration);
    }

    private async Task RunRegistrationActionAsync(
        Func<Task> action,
        string successMessage,
        Guid tournamentId,
        long requestGeneration)
    {
        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return;

        _isSubmitting = true;
        _registrationError = null;
        try
        {
            try
            {
                if(!IsCurrentRequest(tournamentId, requestGeneration))
                    return;

                await action();
                if(!IsCurrentRequest(tournamentId, requestGeneration))
                    return;

                ToastService.ShowSuccess(successMessage);
            }
            catch(Exception exception) when(IsUnauthorized(exception))
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _registrationError = "You are not authorized to change this registration.";
                    ToastService.ShowError(_registrationError);
                }
                return;
            }
            catch(Exception exception)
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _registrationError = GetErrorMessage(exception, "The registration could not be changed right now.");
                    ToastService.ShowError(_registrationError);
                }
                return;
            }

            try
            {
                var currentUserStateLoaded = await RefreshTournamentAsync(
                    tournamentId,
                    expectedRequestGeneration: requestGeneration,
                    preserveSubmitting: true);

                if(ShouldReportSavedMutationRefreshFailure(
                       currentUserStateLoaded,
                       IsCurrentTournament(tournamentId)))
                {
                    if(IsCurrentRequest(tournamentId, requestGeneration))
                    {
                        _registrationError =
                            "The registration was saved, but the displayed registration state could not be refreshed. Try again.";
                        ToastService.ShowWarning(_registrationError);
                    }
                }
            }
            catch(Exception exception)
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _registrationError = $"The registration was saved, but this page could not refresh. Try again. ({GetErrorMessage(exception, "refresh failed")})";
                    ToastService.ShowWarning(_registrationError);
                }
            }
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
            {
                _isSubmitting = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task RemoveAdminRegistrationAsync(AdminTournamentRegistrationDTO registration)
    {
        if(_pendingAdminRemovalRegistrationId.HasValue || _isLoadingRegistration || _isSubmitting)
            return;

        _pendingAdminRemovalRegistrationId = registration.Id;
        _adminError = null;
        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;
        _isSubmitting = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            try
            {
                if(!IsCurrentRequest(tournamentId, requestGeneration))
                    return;

                if(registration.Kind == TournamentRegistrationKind.Individual && registration.User is not null)
                {
                    await TournamentService.RemoveTournamentUserRegistrationAsAdminAsync(
                        tournamentId,
                        registration.User.Id,
                        _adminRemovalReason);
                }
                else if(registration.Kind == TournamentRegistrationKind.Team && registration.Team is not null)
                {
                    await TournamentService.RemoveTournamentTeamRegistrationAsAdminAsync(
                        tournamentId,
                        registration.Team.Id,
                        _adminRemovalReason);
                }
                else
                {
                    if(IsCurrentRequest(tournamentId, requestGeneration))
                        _adminError = "This registration does not contain enough identity data to remove it.";
                    return;
                }

                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _adminRemovalReason = string.Empty;
                    ToastService.ShowSuccess("The registration was removed.");
                }
            }
            catch(Exception exception) when(IsUnauthorized(exception))
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _adminError = "You are not authorized to remove this registration.";
                    ToastService.ShowError(_adminError);
                }
                return;
            }
            catch(Exception exception)
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _adminError = GetErrorMessage(exception, "The registration could not be removed right now.");
                    ToastService.ShowError(_adminError);
                }
                return;
            }

            try
            {
                await RefreshTournamentAsync(
                    tournamentId,
                    expectedRequestGeneration: requestGeneration,
                    preserveSubmitting: true);
            }
            catch(Exception exception)
            {
                if(IsCurrentRequest(tournamentId, requestGeneration))
                {
                    _adminError = $"The registration was removed, but this page could not refresh. Try again. ({GetErrorMessage(exception, "refresh failed")})";
                    ToastService.ShowWarning(_adminError);
                }
            }
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
            {
                _pendingAdminRemovalRegistrationId = null;
                _isSubmitting = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task SelectTeamAsync(Guid teamId)
    {
        if(_isSubmitting || _isLoadingRegistration || _isLoadingTeams || _isLoadingRoster || !CanSelectTeam(teamId))
            return;

        if(_selectedTeamId == teamId && _selectedTeamEligibility is not null)
            return;

        _hasDirtyRosterDraft = false;
        _selectedTeamId = teamId;
        var generation = ++_requestGeneration;
        _isLoadingRegistration = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            await LoadSelectedTeamEligibilityAsync(Tournament.Id, generation);
        }
        finally
        {
            if(IsCurrentRequest(Tournament.Id, generation))
            {
                _isLoadingRegistration = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task HandleTeamChangedAsync(ChangeEventArgs args)
    {
        if(Guid.TryParse(args.Value?.ToString(), out var id))
            await SelectTeamAsync(id);
    }

    private async Task ToggleRosterMemberAsync(Guid userId, ChangeEventArgs args)
    {
        if(_isSubmitting || _isLoadingRegistration || _isLoadingRoster || SelectedTeam?.CaptainUserId == userId)
            return;

        if(args.Value is bool selected && selected)
            _selectedRosterUserIds.Add(userId);
        else
            _selectedRosterUserIds.Remove(userId);

        _hasDirtyRosterDraft = true;
        var generation = ++_requestGeneration;
        await RefreshRosterEligibilityAsync(Tournament.Id, generation, includeCandidateReasons: true);
    }

    private async Task NextTeamStepAsync()
    {
        if(_isSubmitting || _isLoadingRegistration || _isLoadingRoster)
            return;

        var requestGeneration = _requestGeneration;
        if(_activeTeamStep == 0)
        {
            if(!CanAdvanceFromTeamSelection)
                return;
        }
        else if(_activeTeamStep == 1)
        {
            if(!HasLocalRosterShape)
            {
                _teamError = $"Select exactly {RequiredTeamSize} roster member(s), including the captain, before reviewing.";
                return;
            }

            var request = BuildRosterRequest();
            requestGeneration = ++_requestGeneration;
            if(SelectedTeam is null || !await RevalidateRosterBeforeSubmitAsync(SelectedTeam.Id, request, requestGeneration))
                return;
        }
        else
        {
            return;
        }

        if(!IsCurrentRequest(Tournament.Id, requestGeneration))
            return;

        _activeTeamStep++;
    }

    private Task PreviousTeamStepAsync()
    {
        if(!_isSubmitting && !_isLoadingRegistration && !_isLoadingRoster && _activeTeamStep > 0)
            _activeTeamStep--;

        return Task.CompletedTask;
    }

    private Task HandleTeamStepChangedAsync(int index)
    {
        if(index is >= 0 and <= 2 &&
           (index <= _activeTeamStep ||
            (index == 1 && CanAdvanceFromTeamSelection) ||
            (index == 2 && CanSubmitRoster)))
        {
            _activeTeamStep = index;
        }

        return Task.CompletedTask;
    }

    private void ResetTeamStepper()
    {
        if(!_isSubmitting && !_isLoadingRegistration && !_isLoadingRoster)
            _activeTeamStep = 0;
    }

    private SubmitTeamRosterDTO BuildRosterRequest() => new()
    {
        TeamId = SelectedTeam?.Id ?? Guid.Empty,
        UserIds = _selectedRosterUserIds.ToArray()
    };

    private async Task<bool> RevalidateRosterBeforeSubmitAsync(
        Guid teamId,
        SubmitTeamRosterDTO request,
        long requestGeneration)
    {
        await RefreshRosterEligibilityAsync(Tournament.Id, requestGeneration);
        return IsCurrentRequest(Tournament.Id, requestGeneration) &&
               SelectedTeam?.Id == teamId &&
               IsRosterEligibleForWorkflow &&
               request.UserIds.ToHashSet().SetEquals(_selectedRosterUserIds);
    }

    private async Task<bool> RevalidateIndividualRegistrationAsync(bool registering, long requestGeneration)
    {
        var tournamentId = Tournament.Id;
        _isLoadingRegistration = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var state = await TournamentService.GetCurrentUserTournamentRegistrationStateAsync(tournamentId);
            var eligibility = await TournamentService.CheckIndividualTournamentRegistrationEligibilityAsync(tournamentId);
            if(!IsCurrentRequest(tournamentId, requestGeneration))
                return false;

            _registrationState = state;
            _individualEligibility = eligibility;
            return IsRegistrationOpen && (registering
                ? state.CanRegisterIndividual && eligibility.Eligible && state.IndividualRegistration is null
                : state.CanUnregister && state.IndividualRegistration is not null);
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
                _registrationError = "Your account is not authorized to re-check registration state.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
                _registrationError = GetErrorMessage(exception, "Registration state could not be revalidated. Try again.");
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
            {
                _isLoadingRegistration = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        return false;
    }

    private async Task<bool> ReloadCurrentUserStateAsync(long requestGeneration)
    {
        var tournamentId = Tournament.Id;
        _isLoadingRegistration = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var state = await TournamentService.GetCurrentUserTournamentRegistrationStateAsync(tournamentId);
            if(!IsCurrentRequest(tournamentId, requestGeneration))
                return false;

            _registrationState = state;
            return true;
        }
        catch(Exception exception) when(IsUnauthorized(exception))
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
                _registrationError = "Your account is not authorized to re-check registration state.";
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
                _registrationError = GetErrorMessage(exception, "Registration state could not be revalidated. Try again.");
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
            {
                _isLoadingRegistration = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        return false;
    }

    private async Task RetryRegistrationContextAsync()
    {
        if(_isSubmitting || _isLoadingRegistration || Tournament.Id == Guid.Empty)
            return;

        _hasLoadedForTournament = true;
        await LoadRegistrationContextAsync(preserveRosterDraft: true);
    }

    private async Task RetryTeamRegistrationAsync()
    {
        if(_isSubmitting || _isLoadingRegistration || Tournament.Id == Guid.Empty)
            return;

        _hasLoadedForTournament = true;
        await LoadRegistrationContextAsync(preserveRosterDraft: true);
    }

    private async Task RetryRosterEligibilityAsync()
    {
        if(_isSubmitting || _isLoadingRegistration || _isLoadingRoster || SelectedTeam is null)
            return;

        var tournamentId = Tournament.Id;
        var generation = ++_requestGeneration;
        _teamError = null;
        _rosterEligibilityUnavailable = false;
        await RefreshRosterEligibilityAsync(tournamentId, generation, includeCandidateReasons: true);
    }

    private async Task<bool> ConfirmMutationAsync(
        string title,
        string message,
        string actionText,
        long requestGeneration)
    {
        if(_isSubmitting || _isDisposed)
            return false;

        _isSubmitting = true;
        await InvokeAsync(StateHasChanged);
        try
        {
            var result = await DialogService.ShowMessageBoxAsync(
                title,
                message,
                yesText: actionText,
                noText: "Cancel");
            return result == true;
        }
        finally
        {
            if(IsCurrentRequest(Tournament.Id, requestGeneration))
            {
                _isSubmitting = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task HandleTeamStateInvalidatedAsync()
    {
        if(_isDisposed)
            return;

        await InvokeAsync(() => _teamStateInvalidationGate.RunAsync(RefreshAfterTeamInvalidationAsync));
    }

    private async Task RefreshAfterTeamInvalidationAsync()
    {
        if(_isDisposed ||
           !_isAuthenticated ||
           _isSubmitting ||
           Tournament.ParticipationMode != ParticipationMode.Team)
            return;

        var tournamentId = Tournament.Id;
        var requestGeneration = ++_requestGeneration;
        var preserveRosterDraft = HasDirtyRosterDraft;
        _isLoadingRegistration = true;
        _registrationError = null;
        await InvokeAsync(StateHasChanged);
        try
        {
            await RefreshTournamentAsync(
                tournamentId,
                preserveRosterDraft,
                requestGeneration);
        }
        catch(Exception exception)
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
                _teamError = GetErrorMessage(exception, "Team registration state could not be refreshed. Try again.");
        }
        finally
        {
            if(IsCurrentRequest(tournamentId, requestGeneration))
            {
                _isLoadingRegistration = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task<bool> RefreshTournamentAsync(
        Guid tournamentId,
        bool preserveRosterDraft = false,
        long? expectedRequestGeneration = null,
        bool preserveSubmitting = false)
    {
        var requestGeneration = expectedRequestGeneration ?? _requestGeneration;
        var updatedTournament = await TournamentService.GetTournamentByIdAsync(tournamentId);
        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return false;

        if(updatedTournament is null)
            throw new InvalidOperationException("The tournament could not be found while refreshing registration state.");

        Tournament = updatedTournament;
        _loadedRegistrationFingerprint = GetRegistrationFingerprint(updatedTournament);
        _participants.Clear();
        _participants.AddRange(BuildParticipants(updatedTournament));

        if(!IsCurrentRequest(tournamentId, requestGeneration))
            return false;

        await OnTournamentUpdated.InvokeAsync(updatedTournament);

        if(IsCurrentRequest(tournamentId, requestGeneration))
            return await LoadRegistrationContextAsync(
                preserveRosterDraft,
                preserveSubmitting,
                preserveSubmitting ? requestGeneration : null);

        return false;
    }

    internal static bool ShouldReportSavedMutationRefreshFailure(bool currentUserStateLoaded, bool isCurrentRoute) =>
        isCurrentRoute && !currentUserStateLoaded;

    internal static bool ShouldPreserveRosterDraftAfterRefresh(Guid? draftTeamId, Guid? selectedTeamId) =>
        draftTeamId.HasValue && draftTeamId == selectedTeamId;

    internal static IReadOnlyList<IReadOnlyList<Guid>> BatchRosterEligibilityUserIds(IEnumerable<Guid> userIds)
    {
        var batches = new List<IReadOnlyList<Guid>>();
        var batch = new List<Guid>(MaxRosterEligibilityUserIds);

        foreach(var userId in userIds.Distinct())
        {
            batch.Add(userId);
            if(batch.Count < MaxRosterEligibilityUserIds)
                continue;

            batches.Add(batch);
            batch = new List<Guid>(MaxRosterEligibilityUserIds);
        }

        if(batch.Count > 0)
            batches.Add(batch);

        return batches;
    }

    internal static IReadOnlyList<Guid> ReconcileRosterForCurrentCaptain(
        IEnumerable<Guid> selectedUserIds,
        Guid captainUserId)
    {
        var reconciled = selectedUserIds.Distinct().ToList();
        if(captainUserId == Guid.Empty)
            return reconciled;

        if(!reconciled.Contains(captainUserId))
            reconciled.Add(captainUserId);

        return reconciled;
    }

    internal static IReadOnlyList<Guid> MergeEditableRosterCandidateIds(
        IEnumerable<Guid> existingCandidateIds,
        IEnumerable<Guid> selectedRosterUserIds,
        IEnumerable<Guid> eligibilityCandidateIds) =>
        existingCandidateIds
            .Concat(selectedRosterUserIds)
            .Concat(eligibilityCandidateIds)
            .Distinct()
            .ToArray();

    internal static string GetCaptainTransferWarning(int rosterCount, int requiredTeamSize) =>
        rosterCount > requiredTeamSize
            ? $"The team captain changed. The current captain was added to this roster, which now has {rosterCount} of the required {requiredTeamSize} members. Choose a member to remove before saving."
            : "The team captain changed. The current captain was added to this roster. Review the roster before saving.";

    private void MergeRosterCandidateEligibility(IEnumerable<RosterCandidateEligibilityDTO>? candidates)
    {
        foreach(var candidate in (candidates ?? []).OrderBy(candidate => candidate.UserId))
        {
            if(!_rosterCandidatesById.TryGetValue(candidate.UserId, out var existing))
            {
                _rosterCandidatesById[candidate.UserId] = candidate;
                continue;
            }

            _rosterCandidatesById[candidate.UserId] = new RosterCandidateEligibilityDTO
            {
                UserId = candidate.UserId,
                User = existing.User ?? candidate.User,
                Eligible = existing.Eligible && candidate.Eligible,
                ReasonCodes = existing.ReasonCodes
                    .Concat(candidate.ReasonCodes)
                    .Where(reason => !string.IsNullOrWhiteSpace(reason))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(reason => reason, StringComparer.Ordinal)
                    .ToArray()
            };
        }
    }

    private bool IsCurrentRequest(Guid tournamentId, long requestGeneration) =>
        !_isDisposed &&
        IsRequestCurrent(
            tournamentId,
            Tournament.Id,
            requestGeneration,
            _requestGeneration);

    internal static bool IsRequestCurrent(
        Guid expectedTournamentId,
        Guid currentTournamentId,
        long expectedRequestGeneration,
        long currentRequestGeneration) =>
        expectedTournamentId == currentTournamentId &&
        expectedRequestGeneration == currentRequestGeneration;

    private bool IsCurrentTournament(Guid tournamentId) =>
        !_isDisposed && Tournament.Id == tournamentId;

    private EligibilityResponseDTO? GetTeamEligibility(Guid teamId) =>
        _teamEligibilityById.TryGetValue(teamId, out var eligibility) ? eligibility : null;

    private bool CanSelectTeam(Guid teamId)
    {
        var eligibility = GetTeamEligibility(teamId);
        return eligibility is null || CanUseTeamEligibility(teamId, eligibility);
    }

    private bool CanUseTeamEligibility(Guid teamId, EligibilityResponseDTO eligibility) =>
        eligibility.Eligible ||
        (IsCurrentCaptainManagedTeam(teamId) &&
         eligibility.ReasonCodes.Count > 0 &&
         eligibility.ReasonCodes.All(IsExistingRegistrationConflictCode));

    private bool IsCurrentCaptainManagedTeam(Guid teamId) =>
        (_registrationState?.CaptainManagedRegistrations ?? [])
            .Any(registration => registration.Team?.Id == teamId);

    private RosterCandidateEligibilityDTO? GetRosterCandidate(Guid userId) =>
        _rosterCandidatesById.TryGetValue(userId, out var candidate) ? candidate : null;

    private bool IsExistingRosterConflictAllowed(Guid userId, RosterCandidateEligibilityDTO candidate)
    {
        var existingUserIds = (CaptainManagedRegistration?.RosterMembers ?? [])
            .Where(member => member.User is not null)
            .Select(member => member.User.Id)
            .ToHashSet();
        return existingUserIds.Contains(userId) &&
               candidate.ReasonCodes.Count > 0 &&
               candidate.ReasonCodes.All(code => code == "duplicate_participation");
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
        return reasons.Count > 0 && reasons.All(IsExistingRegistrationConflictCode);
    }

    private static bool IsExistingRegistrationConflictCode(string code) =>
        ExistingRegistrationConflictCodes.Contains(code);

    private static string GetConfirmationLabel(RosterMemberConfirmationStatus status) => status switch
    {
        RosterMemberConfirmationStatus.AutoConfirmed => "Captain confirmed",
        RosterMemberConfirmationStatus.Pending => "Pending confirmation",
        RosterMemberConfirmationStatus.Confirmed => "Confirmed",
        _ => status.ToString()
    };

    private string GetRegistrationStateMessage()
    {
        if(!IsRegistrationOpen)
            return "Registration is closed because this tournament is no longer scheduled.";

        if(!_isAuthenticated)
            return "Sign in to check eligibility and manage your registration.";

        if(Tournament.ParticipationMode == ParticipationMode.Individual)
        {
            return _registrationState?.IndividualRegistration is not null
                ? "Your individual registration is active."
                : "Register yourself while the tournament is scheduled.";
        }

        if(HasCaptainManagedRegistration)
            return "Review or edit your captain-managed team registration.";

        return CurrentTeamRegistration?.Team is { } team
            ? $"You are registered on {team.Name}; roster changes belong to its captain."
            : "Team captains can submit an eligible roster. Every selected non-captain must confirm.";
    }

    private static string GetCurrentTeamRegistrationStatusText(TournamentRegistrationStatus status) => status switch
    {
        TournamentRegistrationStatus.PendingConfirmation => "Your team is waiting for all selected members to confirm.",
        TournamentRegistrationStatus.Active => "Your team registration is active.",
        _ => $"Your team registration is {status.ToString().ToLowerInvariant()}."
    };

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

    internal sealed class TeamStateInvalidationGate
    {
        private readonly object _sync = new();
        private bool _isRunning;
        private bool _hasPendingRequest;

        public async Task RunAsync(Func<Task> action)
        {
            lock(_sync)
            {
                if(_isRunning)
                {
                    _hasPendingRequest = true;
                    return;
                }

                _isRunning = true;
            }

            try
            {
                while(true)
                {
                    await action();

                    lock(_sync)
                    {
                        if(!_hasPendingRequest)
                        {
                            _isRunning = false;
                            return;
                        }

                        _hasPendingRequest = false;
                    }
                }
            }
            catch
            {
                lock(_sync)
                {
                    _isRunning = false;
                    _hasPendingRequest = false;
                }

                throw;
            }
        }
    }

    private string GetParticipantModalSummary() =>
        _selectedParticipant?.ParticipationMode == ParticipationMode.Team
            ? "Team roster, captain, and registered members."
            : "Player profile and connected account details.";

    private void ResetRegistrationContext()
    {
        _isLoadingRegistration = false;
        _isLoadingTeams = false;
        _isLoadingRoster = false;
        _isLoadingAdmin = false;
        _isSubmitting = false;
        _registrationState = null;
        _individualEligibility = null;
        _selectedTeamEligibility = null;
        _rosterEligibility = null;
        _teamSummary = null;
        _selectedTeamId = null;
        _pendingAdminRemovalRegistrationId = null;
        _selectedRosterUserIds.Clear();
        _teamEligibilityById.Clear();
        _rosterCandidatesById.Clear();
        _adminRegistrations.Clear();
        _registrationError = null;
        _registrationWarning = null;
        _adminError = null;
        _teamError = null;
        _isAuthenticated = false;
        _isAdmin = false;
        _activeTeamStep = 0;
        _hasDirtyRosterDraft = false;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _requestGeneration++;
        TeamRealtimeService.TeamStateInvalidated -= HandleTeamStateInvalidatedAsync;
    }
}
