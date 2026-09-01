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

    private string TournamentSummary
    {
        get
        {
            if(_tournament == null)
                return string.Empty;

            var competitionType = _tournament.ParticipationMode == ParticipationMode.Team ? "team-based" : "solo";
            return $"Mercurius LAN {competitionType} competition with {_tournament.BracketType.GetLabel().ToLowerInvariant()} structure and {_tournament.Format.GetLabel().ToLowerInvariant()} match format.";
        }
    }

    private string ScheduleSummary
    {
        get
        {
            if(_tournament == null)
                return string.Empty;

            if(!ScheduledMatches.Any())
                return "No estimated matches are currently available yet.";

            var visibleMatches = FilteredScheduledMatches;
            var visibleCount = visibleMatches.Count;
            return $"{visibleCount} match{(visibleCount == 1 ? string.Empty : "es")} currently have estimated timing.";
        }
    }

    private string ScheduleCountLabel =>
        $"{FilteredScheduledMatches.Count} match{(FilteredScheduledMatches.Count == 1 ? string.Empty : "es")}";

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

            _loadError = "Sign in to load this tournament.";
        }
        catch(ApiException exception)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _loadError = string.IsNullOrWhiteSpace(exception.Content)
                ? "Could not load this tournament right now."
                : exception.Content;
            ToastService.ShowError(_loadError);
        }
        catch(Exception)
        {
            if(!IsCurrentLoad(tournamentId, loadGeneration))
                return;

            _loadError = "Could not load this tournament right now.";
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
            "Tournament successfully finished.");
    }

    private Task StartTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.InProgress),
            "Tournament successfully started.");
    }

    private Task CancelTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.Canceled),
            "Tournament successfully canceled.");
    }

    private Task ResetTournamentAsync()
    {
        var tournamentId = TournamentId;
        return ExecuteTournamentActionAsync(
            tournamentId,
            () => TournamentService.SetTournamentLifecycleStateAsync(tournamentId, TournamentStatus.Scheduled),
            "Tournament successfully reset.");
    }

    private async Task DeleteTournamentAsync()
    {
        if(_isActionRunning)
            return;

        var tournamentId = TournamentId;
        var tournamentName = _tournament?.Name ?? "Tournament";
        var actionGeneration = ++_tournamentActionGeneration;
        _isActionRunning = true;
        try
        {
            await TournamentService.DeleteTournamentAsync(tournamentId);
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowSuccess($"{tournamentName} successfully deleted.");
            Navigation.NavigateTo("/tournaments");
        }
        catch(ApiException ex)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(string.IsNullOrWhiteSpace(ex.Content) ? "The tournament could not be deleted." : ex.Content);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError("You are not authorized to delete this tournament.");
        }
        catch(Exception)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError("The tournament could not be deleted right now.");
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
        catch(ApiException ex)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError(string.IsNullOrWhiteSpace(ex.Content) ? "The tournament action could not be completed." : ex.Content);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError("You are not authorized to change this tournament.");
        }
        catch(Exception)
        {
            if(!IsCurrentAction(tournamentId, actionGeneration))
                return;

            ToastService.ShowError("The tournament action could not be completed right now.");
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

    private static string GetPartnerEyebrow(TournamentSponsorPlacement placement)
    {
        return placement.SponsorTier == SponsorTier.Presenting
            ? "Presented by"
            : $"{placement.SponsorTier.GetShortLabel()} partner";
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
            ToastService.ShowWarning("Registrations are closed, the tournament has already started.");
            return;
        }

        Navigation.NavigateTo($"/tournaments/{_tournament.Id}#tournament-participants");
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToLocalDisplayTime().ToString("dd MMM yyyy · HH:mm");
    }

    private string GetScheduleEmptyMessage()
    {
        if(_tournament?.PlannedStartTime is DateTime plannedStart)
            return $"No estimated match times are available yet. Tournament planning starts {FormatDateTime(plannedStart)}.";

        return "No estimated match times are available yet.";
    }

    private string GetMatchTitle(Match match)
    {
        return $"{GetMatchParticipantName(match, true)} vs {GetMatchParticipantName(match, false)}";
    }

    private string GetMatchTimeRange(Match match)
    {
        if(!match.EstimatedStartTime.HasValue)
            return "Estimate unavailable";

        if(!match.EstimatedEndTime.HasValue || match.EstimatedEndTime <= match.EstimatedStartTime)
            return $"Start time {FormatDateTime(match.EstimatedStartTime.Value)}";

        return $"Start time {FormatDateTime(match.EstimatedStartTime.Value)} - {match.EstimatedEndTime.Value.ToLocalDisplayTime():HH:mm}";
    }

    private string GetMatchStageSummary(Match match)
    {
        var bracketLabel = GetScheduleBracketLabel(match);
        return $"{bracketLabel} · Match {match.MatchNumber}";
    }

    private string GetRoundLabel(Match match)
    {
        return $"Round {match.RoundNumber}";
    }

    private string GetScheduleStatus(Match match)
    {
        if(IsMatchDecided(match))
            return "Decided";

        return match.EstimatedStartTime.HasValue ? "Estimated" : "Awaiting estimate";
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
            return "TBD";

        if(firstParticipant && match.Participant1IsBYE)
            return "BYE";

        if(!firstParticipant && match.Participant2IsBYE)
            return "BYE";

        return _tournament.ParticipationMode switch
        {
            ParticipationMode.Team => _participantLookup.ResolveName(ParticipationMode.Team, firstParticipant ? match.TeamParticipant1Id : match.TeamParticipant2Id),
            ParticipationMode.Individual => _participantLookup.ResolveName(ParticipationMode.Individual, firstParticipant ? match.UserParticipant1Id : match.UserParticipant2Id),
            _ => "TBD"
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
            return "Grand final";

        return match.IsLowerBracketMatch ? "Lower bracket" : "Main bracket";
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
            ScheduleBracketFilter.Main => "Main bracket",
            ScheduleBracketFilter.Lower => "Lower bracket",
            ScheduleBracketFilter.GrandFinal => "Grand final",
            _ => "All brackets"
        };

    private void OpenMatchDetails(Match match)
    {
        _selectedMatch = match;
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
            ToastService.ShowSuccess("Tournament sponsor updated.");
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException ex)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = string.IsNullOrWhiteSpace(ex.Content)
                ? "The tournament sponsor could not be updated."
                : ex.Content;
            ToastService.ShowError(_sponsorError);
        }
        catch(UnauthorizedAccessException)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = "You are not authorized to update this tournament sponsor.";
            ToastService.ShowError(_sponsorError);
        }
        catch(Exception)
        {
            if(!IsCurrentSponsorAction(tournamentId, actionGeneration))
                return;

            _sponsorError = "The tournament sponsor could not be updated right now.";
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
