using Blazored.Toast.Services;
using System.Net;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments;

public partial class TournamentDetail : IDisposable
{
    private enum ScheduleBracketFilter
    {
        All,
        Main,
        Lower,
        GrandFinal
    }

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private ISponsorService SponsorService { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;

    [Parameter] public Guid TournamentId { get; set; }

    private TournamentExtended? _tournament;
    private Match? _selectedMatch;
    private int? _selectedSponsorId;
    private List<Sponsor> _availableSponsors = [];
    private bool _isLoading = true;
    private bool _isActionRunning;
    private bool _isSavingSponsor;
    private string? _loadError;
    private bool _notFound;
    private string? _sponsorError;
    private ScheduleBracketFilter _selectedScheduleBracket = ScheduleBracketFilter.All;
    private int? _selectedScheduleRound;
    private TournamentParticipantLookup _participantLookup = TournamentParticipantLookup.Empty;
    private CancellationTokenSource? _loadCancellation;
    private Guid? _loadedTournamentId;
    private long _loadGeneration;
    private long _tournamentActionGeneration;
    private long _sponsorActionGeneration;
    private bool _isDisposed;
    private bool _isRegistrationDialogOpen;

    private IReadOnlyList<Match> ScheduledMatches =>
        _tournament?.Matches
            .Where(IsScheduledMatch)
            .OrderBy(match => !match.EstimatedStartTime.HasValue ? 1 : 0)
            .ThenBy(match => match.EstimatedStartTime ?? DateTime.MaxValue)
            .ThenBy(match => match.RoundNumber)
            .ThenBy(match => match.MatchNumber)
            .ToList() ?? [];

    private IReadOnlyList<Match> FilteredScheduledMatches =>
        ScheduledMatches
            .Where(MatchesSelectedBracket)
            .Where(match => !_selectedScheduleRound.HasValue || match.RoundNumber == _selectedScheduleRound.Value)
            .ToList();

    private IReadOnlyList<int> AvailableScheduleRounds =>
        ScheduledMatches
            .Where(MatchesSelectedBracket)
            .Select(match => match.RoundNumber)
            .Distinct()
            .OrderBy(round => round)
            .ToList();

    private TournamentSponsorPlacement? FeaturedPartner =>
        _tournament?.SponsorPlacement;

    private Sponsor? SelectedSponsor =>
        _selectedSponsorId.HasValue
            ? _availableSponsors.FirstOrDefault(sponsor => sponsor.Id == _selectedSponsorId.Value)
            : null;

    private string ScheduleSummary
    {
        get
        {
            if(_tournament == null)
                return string.Empty;

            if(!ScheduledMatches.Any())
                return Localization["Feature.tournaments.noEstimatedMatches"];

            var visibleMatches = FilteredScheduledMatches;
            var visibleCount = visibleMatches.Count;
            return Localization.Get("Feature.tournaments.scheduleSummary", visibleCount);
        }
    }

    private string ScheduleCountLabel =>
        Localization.Get("Feature.tournaments.scheduleCount", FilteredScheduledMatches.Count);

    protected override Task OnParametersSetAsync()
    {
        if(_loadedTournamentId == TournamentId)
            return Task.CompletedTask;

        _loadedTournamentId = TournamentId;
        ResetForTournamentChange();
        return LoadTournamentDataAsync(TournamentId);
    }

    private void ResetForTournamentChange()
    {
        _tournament = null;
        _selectedMatch = null;
        _selectedSponsorId = null;
        _participantLookup = TournamentParticipantLookup.Empty;
        _selectedScheduleBracket = ScheduleBracketFilter.All;
        _selectedScheduleRound = null;
        _loadError = null;
        _notFound = false;
        _sponsorError = null;
        ++_tournamentActionGeneration;
        ++_sponsorActionGeneration;
        _isActionRunning = false;
        _isSavingSponsor = false;
    }

    private async Task LoadTournamentDataAsync(Guid tournamentId)
    {
        _loadCancellation?.Cancel();
        var loadCancellation = new CancellationTokenSource();
        _loadCancellation = loadCancellation;
        var loadGeneration = ++_loadGeneration;

        _isLoading = true;
        _loadError = null;
        _notFound = false;
        try
        {
            var tournament = await TournamentService.GetTournamentByIdAsync(tournamentId, loadCancellation.Token);
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _tournament = tournament;
            if(_tournament is null)
            {
                _notFound = true;
                return;
            }

            ReconcileSelectedMatchProjection();
            _participantLookup = TournamentParticipantLookup.FromTournament(_tournament);
            SyncSelectedSponsor();

            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            if(authenticationState.User.Identity?.IsAuthenticated == true &&
               authenticationState.User.IsInRole("admin") &&
               _availableSponsors.Count == 0)
            {
                try
                {
                    var sponsors = await SponsorService.GetSponsorsAsync();
                    if(!IsCurrentLoad(tournamentId, loadGeneration))
                        return;

                    _availableSponsors = sponsors
                        .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                        .ThenBy(sponsor => sponsor.Name)
                        .ToList();
                }
                catch(Exception)
                {
                    if(!IsCurrentLoad(tournamentId, loadGeneration))
                        return;

                    // Sponsor administration is supplementary; keep the tournament detail usable.
                    _availableSponsors = [];
                }
            }
        }
        catch(OperationCanceledException) when(loadCancellation.IsCancellationRequested)
        {
            // A newer tournament parameter or retry superseded this request.
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _notFound = true;
            _tournament = null;
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _loadError = Localization["Feature.tournaments.signInToLoad"];
        }
        catch(ApiException)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _loadError = Localization["Feature.tournaments.loadError"];
            ToastService.ShowError(_loadError);
        }
        catch(Exception)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _loadError = Localization["Feature.tournaments.loadError"];
            ToastService.ShowError(_loadError);
        }
        finally
        {
            if(IsCurrentLoad(tournamentId, loadGeneration))
            {
                _loadCancellation = null;
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }

            loadCancellation.Dispose();
        }
    }

    private Task RetryLoadAsync() => LoadTournamentDataAsync(TournamentId);

    private Task LoadTournamentDataAsync() => LoadTournamentDataAsync(TournamentId);

    private bool IsCurrentLoad(Guid tournamentId, long loadGeneration) =>
        !_isDisposed &&
        loadGeneration == _loadGeneration &&
        tournamentId == TournamentId;

    private bool IsCurrentAction(Guid tournamentId, long actionGeneration) =>
        !_isDisposed &&
        actionGeneration == _tournamentActionGeneration &&
        tournamentId == TournamentId;

    private bool IsCurrentSponsorAction(Guid tournamentId, long actionGeneration) =>
        !_isDisposed &&
        actionGeneration == _sponsorActionGeneration &&
        tournamentId == TournamentId;

    private Task HandleTournamentUpdated(TournamentExtended updatedTournament)
    {
        if(updatedTournament.Id != TournamentId)
            return Task.CompletedTask;

        _tournament = updatedTournament;
        ReconcileSelectedMatchProjection();
        _participantLookup = TournamentParticipantLookup.FromTournament(_tournament);
        SyncSelectedSponsor();
        return InvokeAsync(StateHasChanged);
    }

    private Task FinishTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.Completed),
            Localization["Feature.tournaments.finished"]);
    }

    private Task StartTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.InProgress),
            Localization["Feature.tournaments.started"]);
    }

    private Task CancelTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.Canceled),
            Localization["Feature.tournaments.canceled"]);
    }

    private Task ResetTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.Scheduled),
            Localization["Feature.tournaments.resetDone"]);
    }

    private async Task DeleteTournamentAsync()
    {
        if(_isActionRunning)
            return;

        var tournamentId = TournamentId;
        var tournamentName = _tournament?.Name ?? Localization["Feature.tournaments.tournamentFallback"];
        var actionGeneration = ++_tournamentActionGeneration;
        _isActionRunning = true;
        try
        {
            await TournamentService.DeleteTournamentAsync(tournamentId);
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowSuccess(Localization.Get("Feature.tournaments.deleted", tournamentName));
            Navigation.NavigateTo("/tournaments");
        }
        catch(ApiException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.deleteFailed"]);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.deleteUnauthorized"]);
        }
        catch(Exception)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.deleteFailed"]);
        }
        finally
        {
            if(IsCurrentAction(tournamentId, actionGeneration))
                _isActionRunning = false;
        }
    }

    private async Task ExecuteTournamentActionAsync(Guid tournamentId, Func<Task> tournamentAction, string successMessage)
    {
        if(_isActionRunning)
            return;

        var actionGeneration = ++_tournamentActionGeneration;
        _isActionRunning = true;
        try
        {
            await tournamentAction();
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowSuccess(successMessage);
            await LoadTournamentDataAsync(tournamentId);
        }
        catch(ApiException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.actionFailed"]);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.actionUnauthorized"]);
        }
        catch(Exception)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(Localization["Feature.tournaments.actionFailed"]);
        }
        finally
        {
            if(IsCurrentAction(tournamentId, actionGeneration))
                _isActionRunning = false;
        }
    }

    private string GetImageUrl(string? imageUrl)
    {
        return AssetUrlResolver.Resolve(Configuration, imageUrl);
    }

    private string GetSponsorLogoUrl(string? imageUrl)
    {
        return AssetUrlResolver.Resolve(Configuration, imageUrl);
    }

    private string GetFeaturedPartnerSummary(TournamentSponsorPlacement placement)
    {
        return placement.SponsorDescription ?? string.Empty;
    }

    private string GetPartnerEyebrow(TournamentSponsorPlacement placement)
    {
        return placement.SponsorTier == SponsorTier.Presenting
            ? Localization["Feature.tournaments.presentedBy"]
            : Localization.Get("Feature.tournaments.tierPartner", GetSponsorTierShortLabel(placement.SponsorTier));
    }

    private string GetPageAnchorUrl(string anchorId)
    {
        return _tournament == null
            ? "/tournaments"
            : $"/tournaments/{_tournament.Id}#{anchorId}";
    }

    private void NavigateToRegister()
    {
        if(_tournament == null)
            return;

        if(!CanRegister(_tournament))
        {
            ToastService.ShowWarning(Localization["tournament.registrationClosedShort"]);
            return;
        }

        _isRegistrationDialogOpen = true;
    }

    private Task HandleRegistrationDialogOpenChanged(bool isOpen)
    {
        _isRegistrationDialogOpen = isOpen;
        return Task.CompletedTask;
    }

    private string FormatDateTime(DateTime dateTime)
    {
        return Localization.FormatDateTime(dateTime.ToLocalDisplayTime());
    }

    private string GetScheduleEmptyMessage()
    {
        if(_tournament?.PlannedStartTime is DateTime plannedStart)
            return Localization.Get("Feature.tournaments.scheduleStarts", FormatDateTime(plannedStart));

        return Localization["Feature.tournaments.noEstimatedMatchTimes"];
    }

    private string GetMatchTitle(Match match)
    {
        return Localization.Get(
            "Feature.tournaments.vs",
            GetMatchParticipantName(match, true),
            GetMatchParticipantName(match, false));
    }

    private string GetMatchTimeRange(Match match)
    {
        if(!match.EstimatedStartTime.HasValue)
            return Localization["Feature.tournaments.estimateUnavailable"];

        if(!match.EstimatedEndTime.HasValue || match.EstimatedEndTime <= match.EstimatedStartTime)
            return Localization.Get("Feature.tournaments.startTimeAt", FormatDateTime(match.EstimatedStartTime.Value));

        return Localization.Get(
            "Feature.tournaments.startTimeRange",
            FormatDateTime(match.EstimatedStartTime.Value),
            Localization.FormatTime(match.EstimatedEndTime.Value.ToLocalDisplayTime()));
    }

    private string GetMatchStageSummary(Match match)
    {
        var bracketLabel = GetScheduleBracketLabel(match);
        return Localization.Get("Feature.tournaments.matchStageSummary", bracketLabel, match.MatchNumber);
    }

    private string GetRoundLabel(Match match)
    {
        return Localization.Get("Feature.tournaments.roundNumber", match.RoundNumber);
    }

    private string GetScheduleStatus(Match match)
    {
        if(IsMatchDecided(match))
            return Localization["Feature.tournaments.decided"];

        return match.EstimatedStartTime.HasValue
            ? Localization["Feature.tournaments.estimated"]
            : Localization["Feature.tournaments.awaitingEstimate"];
    }

    private string GetScheduleStatusClass(Match match)
    {
        if(IsMatchDecided(match))
            return "Tournament-schedule-status--complete";

        return match.EstimatedStartTime.HasValue ? "Tournament-schedule-status--scheduled" : "Tournament-schedule-status--pending";
    }

    private string GetMatchParticipantName(Match match, bool firstParticipant)
    {
        if(_tournament == null)
            return Localization["Feature.tournaments.tbd"];

        if(firstParticipant && match.Participant1IsBYE)
            return Localization["Feature.tournaments.bye"];

        if(!firstParticipant && match.Participant2IsBYE)
            return Localization["Feature.tournaments.bye"];

        return _tournament.ParticipationMode switch
        {
            ParticipationMode.Team => _participantLookup.ResolveName(ParticipationMode.Team, firstParticipant ? match.TeamParticipant1Id : match.TeamParticipant2Id),
            ParticipationMode.Individual => _participantLookup.ResolveName(ParticipationMode.Individual, firstParticipant ? match.UserParticipant1Id : match.UserParticipant2Id),
            _ => Localization["Feature.tournaments.tbd"]
        };
    }

    private static bool IsMatchDecided(Match match)
    {
        return match.UserWinnerId.HasValue || match.TeamWinnerId.HasValue;
    }

    private static bool IsScheduledMatch(Match match)
    {
        return match.EstimatedStartTime.HasValue && !IsMatchDecided(match);
    }

    private static bool CanRegister(Tournament Tournament) => Tournament.Status == TournamentStatus.Scheduled;

    private string GetScheduleBracketLabel(Match match)
    {
        if(IsGrandFinalMatch(match))
            return Localization["Feature.tournaments.grandFinal"];

        return match.IsLowerBracketMatch ? Localization["Feature.tournaments.lowerBracket"] : Localization["Feature.tournaments.mainBracket"];
    }

    private bool IsGrandFinalMatch(Match match)
    {
        return _tournament?.BracketType == BracketType.DoubleElimination &&
            !match.IsLowerBracketMatch &&
            match.RoundNumber == ScheduledMatches.LastOrDefault()?.RoundNumber;
    }

    private bool MatchesSelectedBracket(Match match)
    {
        return _selectedScheduleBracket switch
        {
            ScheduleBracketFilter.Main => !match.IsLowerBracketMatch && !IsGrandFinalMatch(match),
            ScheduleBracketFilter.Lower => match.IsLowerBracketMatch,
            ScheduleBracketFilter.GrandFinal => IsGrandFinalMatch(match),
            _ => true
        };
    }

    private void HandleScheduleBracketChanged(ChangeEventArgs args)
    {
        if(Enum.TryParse<ScheduleBracketFilter>(args.Value?.ToString(), out var selectedBracket))
            _selectedScheduleBracket = selectedBracket;
        else
            _selectedScheduleBracket = ScheduleBracketFilter.All;

        if(_selectedScheduleRound.HasValue && !AvailableScheduleRounds.Contains(_selectedScheduleRound.Value))
            _selectedScheduleRound = null;
    }

    private void HandleScheduleRoundChanged(ChangeEventArgs args)
    {
        var rawValue = args.Value?.ToString();
        _selectedScheduleRound = int.TryParse(rawValue, out var parsedRound) ? parsedRound : null;
    }

    private string GetScheduleBracketFilterLabel(ScheduleBracketFilter bracketFilter) =>
        bracketFilter switch
        {
            ScheduleBracketFilter.Main => Localization["Feature.tournaments.mainBracket"],
            ScheduleBracketFilter.Lower => Localization["Feature.tournaments.lowerBracket"],
            ScheduleBracketFilter.GrandFinal => Localization["Feature.tournaments.grandFinal"],
            _ => Localization["Feature.tournaments.allBrackets"]
        };

    private async Task HandleMatchDataReloadAsync(Match refreshedMatch)
    {
        if(!TryApplyRefreshedMatchProjection(refreshedMatch))
            return;

        // Apply the child refresh before asking the parent to reload. If that reload
        // fails, both the schedule and either bracket view still render the fresh
        // match instead of passing their old same-ID instance back to the dialog.
        await LoadTournamentDataAsync(TournamentId);
    }

    private Task HandleMatchRefreshedAsync(Match refreshedMatch)
    {
        TryApplyRefreshedMatchProjection(refreshedMatch);
        return Task.CompletedTask;
    }

    private bool TryApplyRefreshedMatchProjection(Match refreshedMatch)
    {
        if(_tournament is null || !_tournament.Matches.Any(match => match.Id == refreshedMatch.Id))
            return false;

        _selectedMatch = ApplyRefreshedMatchProjection(_tournament, _selectedMatch, refreshedMatch);
        return true;
    }

    private void ReconcileSelectedMatchProjection()
    {
        if(_tournament is null)
            return;

        _selectedMatch = ReconcileSelectedMatch(_selectedMatch, _tournament);
    }

    private void OpenMatchDetails(Match match)
    {
        _selectedMatch = match;
    }

    internal static Match? ReconcileSelectedMatch(Match? selectedMatch, TournamentExtended tournament) =>
        selectedMatch is null
            ? null
            : tournament.Matches.FirstOrDefault(match => match.Id == selectedMatch.Id);

    internal static Match? PreserveSelectedMatchProjection(Match? selectedMatch, Match refreshedMatch) =>
        selectedMatch?.Id == refreshedMatch.Id
            ? refreshedMatch
            : selectedMatch;

    internal static Match? ApplyRefreshedMatchProjection(
        TournamentExtended tournament,
        Match? selectedMatch,
        Match refreshedMatch)
    {
        tournament.Matches = tournament.Matches
            .Select(match => match.Id == refreshedMatch.Id ? refreshedMatch : match)
            .ToList();
        return PreserveSelectedMatchProjection(selectedMatch, refreshedMatch);
    }

    private void HandleScheduleItemKeyDown(KeyboardEventArgs args, Match match)
    {
        if(args.Key is "Enter" or " ")
        {
            OpenMatchDetails(match);
        }
    }

    private Task CloseMatchDetailsAsync()
    {
        _selectedMatch = null;
        return Task.CompletedTask;
    }

    private async Task SaveSponsorPlacementsAsync()
    {
        if(_tournament == null || _isSavingSponsor)
            return;

        var tournamentId = _tournament.Id;
        var actionGeneration = ++_sponsorActionGeneration;
        _isSavingSponsor = true;
        _sponsorError = null;
        try
        {
            var sponsorPlacements = _selectedSponsorId.HasValue
                ? new List<TournamentSponsorPlacementInputDTO>
                {
                    new()
                    {
                        SponsorId = _selectedSponsorId.Value,
                        Context = SponsorContext.TournamentPartner,
                        DisplayOrder = 1
                    }
                }
                : [];

            var updatedTournament = await TournamentService.ReplaceTournamentSponsorsAsync(tournamentId, new ReplaceTournamentSponsorsDTO
            {
                SponsorPlacements = sponsorPlacements
            });

            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _tournament = updatedTournament;
            _participantLookup = TournamentParticipantLookup.FromTournament(_tournament);
            SyncSelectedSponsor();
            ToastService.ShowSuccess(Localization["Feature.tournaments.sponsorUpdated"]);
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = Localization["Feature.tournaments.sponsorUpdateFailed"];
            ToastService.ShowError(_sponsorError);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = Localization["Feature.tournaments.sponsorUpdateUnauthorized"];
            ToastService.ShowError(_sponsorError);
        }
        catch(Exception)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = Localization["Feature.tournaments.sponsorUpdateFailed"];
            ToastService.ShowError(_sponsorError);
        }
        finally
        {
            if(IsCurrentSponsorAction(tournamentId, actionGeneration))
                _isSavingSponsor = false;
        }
    }

    private void SyncSelectedSponsor()
    {
        _selectedSponsorId = _tournament?.SponsorPlacement?.SponsorId;
    }

    private string GetStatusLabel(TournamentStatus status) => status switch
    {
        TournamentStatus.Scheduled => Localization["Feature.tournament.statusScheduled"],
        TournamentStatus.InProgress => Localization["Feature.tournament.statusInProgress"],
        TournamentStatus.Completed => Localization["Feature.tournament.statusCompleted"],
        TournamentStatus.Canceled => Localization["Feature.tournament.statusCanceled"],
        _ => status.ToString()
    };

    private string GetParticipationLabel(ParticipationMode mode) => mode switch
    {
        ParticipationMode.Individual => Localization["Feature.tournament.participationIndividual"],
        ParticipationMode.Team => Localization["Feature.tournament.participationTeam"],
        _ => mode.ToString()
    };

    private string GetBracketLabel(BracketType bracketType) => bracketType switch
    {
        BracketType.SingleElimination => Localization["Feature.tournament.bracketSingle"],
        BracketType.DoubleElimination => Localization["Feature.tournament.bracketDouble"],
        BracketType.RoundRobin => Localization["Feature.tournament.bracketRoundRobin"],
        BracketType.Swiss => Localization["Feature.tournament.bracketSwiss"],
        _ => bracketType.ToString()
    };

    private string GetFormatLabel(TournamentFormat format) => format switch
    {
        TournamentFormat.BestOf1 => Localization["Feature.tournament.formatBestOf1"],
        TournamentFormat.BestOf3 => Localization["Feature.tournament.formatBestOf3"],
        TournamentFormat.BestOf5 => Localization["Feature.tournament.formatBestOf5"],
        _ => format.ToString()
    };

    private string GetSponsorTierLabel(SponsorTier tier) => Localization.Get(
        "Feature.tournaments.tierPartner",
        GetSponsorTierShortLabel(tier));

    private string GetSponsorTierShortLabel(SponsorTier tier) => tier switch
    {
        SponsorTier.Presenting => Localization["Feature.sponsors.tierPresenting"],
        SponsorTier.Gold => Localization["Feature.sponsors.tierGold"],
        SponsorTier.Silver => Localization["Feature.sponsors.tierSilver"],
        SponsorTier.Bronze => Localization["Feature.sponsors.tierBronze"],
        _ => tier.ToString()
    };

    public void Dispose()
    {
        _isDisposed = true;
        ++_loadGeneration;
        ++_tournamentActionGeneration;
        ++_sponsorActionGeneration;
        var loadCancellation = _loadCancellation;
        _loadCancellation = null;
        loadCancellation?.Cancel();
    }
}
