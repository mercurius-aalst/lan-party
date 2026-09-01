using System.Net;
using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.DetailView;

public partial class TournamentMatchDetailsDialog : IAsyncDisposable
{
    internal enum MatchMutationAction
    {
        ConfirmEnded,
        SubmitScore,
        Forfeit,
        ForceForfeit,
        Resolve,
        Reverse
    }

    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<Match> OnDataReload { get; set; }
    [Parameter] public EventCallback<Match> OnMatchRefreshed { get; set; }
    [Parameter] public string Participant2Name { get; set; } = null!;
    [Parameter] public string Participant1Name { get; set; } = null!;
    [Parameter] public ParticipantViewModel? Participant1 { get; set; }
    [Parameter] public ParticipantViewModel? Participant2 { get; set; }

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private Guid? Participant1Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant1Id : Match.UserParticipant1Id;
    private Guid? Participant2Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant2Id : Match.UserParticipant2Id;
    private Guid? WinnerId => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamWinnerId : Match.UserWinnerId;
    private ParticipantViewModel? HeaderParticipant1 => Participant1 ?? GetParticipantById(Participant1Id);
    private ParticipantViewModel? HeaderParticipant2 => Participant2 ?? GetParticipantById(Participant2Id);
    private TournamentParticipantLookup _participantLookup = TournamentParticipantLookup.Empty;
    private MatchActionStateDTO? _actionState;
    private Guid _loadedMatchId;
    private int _refreshGeneration;
    private bool _hasLoaded;
    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _requiresAuthentication;
    private bool _authorizationDenied;
    private bool _hasFreshActionState;
    private bool _forfeitConfirmationRequested;
    private MatchParticipantSide? _adminForfeitConfirmationSide;
    private bool _reverseConfirmationRequested;
    private string? _errorMessage;
    private int? _participant1Score;
    private int? _participant2Score;
    private bool HasScoreInputs => _participant1Score.HasValue && _participant2Score.HasValue;
    private CancellationTokenSource? _deadlineRefreshCancellation;
    private DateTime? _deadlineRefreshTriggeredFor;
    private Task? _deadlineRefreshTask;
    private Match? _freshMatchProjection;

    protected override async Task OnParametersSetAsync()
    {
        _participantLookup = TournamentParticipantLookup.FromTournament(Tournament);
        if(_loadedMatchId == Match.Id)
        {
            if(_freshMatchProjection is { } freshMatch &&
               freshMatch.Id == Match.Id &&
               Match.ResultVersion < freshMatch.ResultVersion)
                Match = freshMatch;
            else if(_freshMatchProjection is null || Match.ResultVersion >= _freshMatchProjection.ResultVersion)
                _freshMatchProjection = Match;

            return;
        }

        _loadedMatchId = Match.Id;
        _freshMatchProjection = null;
        _hasLoaded = false;
        _actionState = null;
        _errorMessage = null;
        _requiresAuthentication = false;
        _authorizationDenied = false;
        _hasFreshActionState = false;
        _forfeitConfirmationRequested = false;
        _adminForfeitConfirmationSide = null;
        _reverseConfirmationRequested = false;
        StopDeadlineRefresh();
        await RefreshAsync(Match.Id);
    }

    private ParticipantViewModel? GetParticipantById(Guid? participantId) =>
        _participantLookup.Resolve(Match.ParticipationMode, participantId);

    private bool IsWinner(Guid? participantId) => WinnerId != null && participantId == WinnerId;

    private string GetStageLabel() => Match.IsLowerBracketMatch ? "Lower bracket" : "Main bracket";

    private string GetRoundLabel() => $"Round {Match.RoundNumber}";

    private string GetStatusLabel() => Match.LifecycleState switch
    {
        MatchLifecycleState.AwaitingEndedConfirmation => "Awaiting confirmation",
        MatchLifecycleState.AwaitingScore => "Ready for score",
        MatchLifecycleState.ScoreConfirmation => "Score confirmation",
        MatchLifecycleState.Disputed => "Score disputed",
        MatchLifecycleState.AdminResolutionRequired => "Admin resolution required",
        MatchLifecycleState.Completed => "Completed",
        MatchLifecycleState.Forfeited => "Forfeited",
        MatchLifecycleState.Reversed => "Reversed",
        _ => "Awaiting match result"
    };

    private string GetStatusDescription() => Match.LifecycleState switch
    {
        MatchLifecycleState.AwaitingEndedConfirmation => "Both sides must confirm that the match has ended.",
        MatchLifecycleState.AwaitingScore => "Both sides have confirmed the end. Either eligible participant or captain may submit the score.",
        MatchLifecycleState.ScoreConfirmation => "The first score report is saved. The opponent has five minutes to agree or report a correction.",
        MatchLifecycleState.Disputed => "The reports do not match. Each side has one correction opportunity before administrator resolution.",
        MatchLifecycleState.AdminResolutionRequired => "The correction window expired. An authorized tournament administrator must resolve this result.",
        MatchLifecycleState.Completed => "The result is official and has advanced the bracket.",
        MatchLifecycleState.Forfeited => "The result is official after a side forfeited.",
        MatchLifecycleState.Reversed => "The result was reversed. The match can be played again when both sides are assigned.",
        _ => "The authoritative match state is loading."
    };

    private string GetStatusClass() => Match.LifecycleState switch
    {
        MatchLifecycleState.Completed or MatchLifecycleState.Forfeited => "match-status-pill--complete",
        MatchLifecycleState.Disputed or MatchLifecycleState.AdminResolutionRequired => "match-status-pill--attention",
        MatchLifecycleState.Reversed => "match-status-pill--reversed",
        _ => "match-status-pill--scheduled"
    };

    private string GetStartDateTimeLabel() =>
        Match.EstimatedStartTime.HasValue
            ? Match.EstimatedStartTime.Value.ToLocalDisplayTime().ToString("dd MMM yyyy · HH:mm")
            : "Unavailable";

    private string GetDeadlineLabel()
    {
        var deadline = GetDeadlineUtc();
        return deadline.HasValue
            ? $"Window closes {deadline.Value.ToLocalDisplayTime():dd MMM yyyy · HH:mm}"
            : string.Empty;
    }

    private string GetDeadlineRemainingLabel()
    {
        var deadline = GetDeadlineUtc();
        if(!deadline.HasValue)
            return string.Empty;

        var remaining = deadline.Value.ToUniversalTime() - DateTime.UtcNow;
        if(remaining <= TimeSpan.Zero)
            return "Window closed; refreshing authoritative state...";

        var minutes = (int)remaining.TotalMinutes;
        var seconds = remaining.Seconds;
        return minutes > 0
            ? $"About {minutes}m {seconds:00}s remaining"
            : $"About {seconds}s remaining";
    }

    private DateTime? GetDeadlineUtc() => Match.LifecycleState == MatchLifecycleState.Disputed
        ? Match.CorrectionDeadlineUtc
        : Match.ScoreConfirmationDeadlineUtc;

    private string GetCardClass(Guid? participantId)
    {
        if(IsWinner(participantId))
            return "participant-card winner-card";
        if(WinnerId != null && participantId != null)
            return "participant-card loser-card";
        return "participant-card";
    }

    private static string BuildTeamProfileHref(string teamName) =>
        string.IsNullOrWhiteSpace(teamName)
            ? string.Empty
            : $"/teams/{Uri.EscapeDataString(teamName.Trim())}";

    private bool IsAdminResolutionState() =>
        Match.LifecycleState is MatchLifecycleState.Disputed or MatchLifecycleState.AdminResolutionRequired;

    private bool ShowPrivateReports => _hasFreshActionState && _actionState is not null;

    private int? OwnReport1 => _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant1
        ? _actionState.Participant1ReportedScore1
        : _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant2
            ? _actionState.Participant2ReportedScore1
            : null;

    private int? OwnReport2 => _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant1
        ? _actionState.Participant1ReportedScore2
        : _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant2
            ? _actionState.Participant2ReportedScore2
            : null;

    private int? OpponentReport1 => _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant1
        ? _actionState.Participant2ReportedScore1
        : _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant2
            ? _actionState.Participant1ReportedScore1
            : null;

    private int? OpponentReport2 => _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant1
        ? _actionState.Participant2ReportedScore2
        : _actionState?.AuthorizedParticipant == MatchParticipantSide.Participant2
            ? _actionState.Participant1ReportedScore2
            : null;

    private int? PrivateReport1 => _actionState?.Participant1ReportedScore1 ?? _actionState?.Participant2ReportedScore1;

    private int? PrivateReport2 => _actionState?.Participant1ReportedScore2 ?? _actionState?.Participant2ReportedScore2;

    internal static (int? Participant1Score, int? Participant2Score) GetInitialScores(MatchActionStateDTO state)
    {
        var actorReport = state.AuthorizedParticipant switch
        {
            MatchParticipantSide.Participant1 => GetCompleteReport(
                state.Participant1ReportedScore1,
                state.Participant1ReportedScore2),
            MatchParticipantSide.Participant2 => GetCompleteReport(
                state.Participant2ReportedScore1,
                state.Participant2ReportedScore2),
            _ => null
        };
        if(actorReport is { } report)
            return report;

        var officialResult = GetCompleteReport(
            state.Match.Participant1Score,
            state.Match.Participant2Score);
        return officialResult is { } result
            ? (result.Item1, result.Item2)
            : ((int?)null, (int?)null);
    }

    internal static bool ShouldRenderAdminReports(MatchActionStateDTO state) =>
        (state.Participant1ReportedScore1.HasValue ||
         state.Participant1ReportedScore2.HasValue ||
         state.Participant2ReportedScore1.HasValue ||
         state.Participant2ReportedScore2.HasValue);

    private static (int, int)? GetCompleteReport(int? participant1Score, int? participant2Score) =>
        participant1Score is { } score1 && participant2Score is { } score2
            ? (score1, score2)
            : null;

    private string ActionSubjectLabel => Match.ParticipationMode == ParticipationMode.Team
        ? "Your captain actions"
        : "Your player actions";

    private string SignInHref => $"/account/login?returnUrl={Uri.EscapeDataString(GetCurrentRelativeUrl())}";

    private string GetCurrentRelativeUrl()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        return string.IsNullOrWhiteSpace(relativePath)
            ? "/"
            : $"/{relativePath}";
    }

    private string GetParticipantName(MatchParticipantSide side) =>
        side == MatchParticipantSide.Participant1 ? Participant1Name : Participant2Name;

    private static string GetAdminBlockedReason(string? reason) => reason switch
    {
        "tournament_not_in_progress" => "This tournament is no longer in progress.",
        "match_not_completed" => "This match does not have an official result to reverse.",
        "match_already_completed" => "This match already has an official result.",
        "match_not_ready" => "Both match sides must be assigned before an administrator can record a forfeit.",
        "match_not_forfeitable" => "This match cannot be forfeited in its current state.",
        "match_not_disputed" => "This match does not currently require administrator resolution.",
        "match_requires_admin_resolution" => "This match already requires administrator resolution.",
        "match_reversal_blocked" => "This result cannot be reversed because a linked downstream match has already been played or resolved.",
        "downstream_graph_too_large" => "This result cannot be reversed until the linked bracket is reviewed by an administrator.",
        "admin_required" => "An authorized tournament administrator is required for this action.",
        _ => "This administrator action is unavailable in the authoritative match state."
    };

    private static string FormatReport(int? participant1Score, int? participant2Score) =>
        participant1Score.HasValue && participant2Score.HasValue
            ? $"{participant1Score}-{participant2Score}"
            : "Not submitted";

    private async Task<bool> RefreshAsync(Guid expectedMatchId)
    {
        var generation = ++_refreshGeneration;
        _isLoading = true;
        _hasFreshActionState = false;
        _errorMessage = null;
        _requiresAuthentication = false;
        _authorizationDenied = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var state = await TournamentService.GetMatchActionStateAsync(expectedMatchId);
            if(generation != _refreshGeneration || Match.Id != expectedMatchId)
                return false;

            Match = state.Match;
            _freshMatchProjection = Match;
            _actionState = state;
            _hasFreshActionState = true;
            (_participant1Score, _participant2Score) = GetInitialScores(state);
            _hasLoaded = true;
            StartDeadlineRefresh();
            await NotifyMatchRefreshedAsync();
            return true;
        }
        catch(ApiException exception) when(exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            if(generation != _refreshGeneration || Match.Id != expectedMatchId)
                return false;

            try
            {
                var publicMatch = await TournamentService.GetMatchByIdAsync(expectedMatchId);
                if(generation != _refreshGeneration || Match.Id != expectedMatchId)
                    return false;

                Match = publicMatch;
                _freshMatchProjection = Match;
                _actionState = null;
                _hasFreshActionState = false;
                _participant1Score = publicMatch.Participant1Score;
                _participant2Score = publicMatch.Participant2Score;
                _requiresAuthentication = exception.StatusCode == HttpStatusCode.Unauthorized;
                _authorizationDenied = exception.StatusCode == HttpStatusCode.Forbidden;
                _hasLoaded = true;
                StartDeadlineRefresh();
                await NotifyMatchRefreshedAsync();
                return true;
            }
            catch(Exception fallbackException)
            {
                if(generation == _refreshGeneration && Match.Id == expectedMatchId)
                {
                    _errorMessage = GetErrorMessage(
                        fallbackException,
                        "The public match state is unavailable. Retry to continue.");
                    _hasFreshActionState = false;
                    _hasLoaded = true;
                    StopDeadlineRefresh();
                }

                return false;
            }
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound)
        {
            if(generation == _refreshGeneration && Match.Id == expectedMatchId)
            {
                _errorMessage = "This match is no longer available.";
                _hasFreshActionState = false;
                _hasLoaded = true;
            }
            return false;
        }
        catch(Exception exception)
        {
            if(generation == _refreshGeneration && Match.Id == expectedMatchId)
            {
                _errorMessage = GetErrorMessage(exception, "The authoritative match state is unavailable.");
                _hasFreshActionState = false;
                _hasLoaded = true;
            }
            return false;
        }
        finally
        {
            if(generation == _refreshGeneration)
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private async Task NotifyMatchRefreshedAsync()
    {
        if(!OnMatchRefreshed.HasDelegate)
            return;

        try
        {
            await OnMatchRefreshed.InvokeAsync(Match);
        }
        catch(Exception exception)
        {
            ToastService.ShowWarning(GetErrorMessage(
                exception,
                "The latest match state could not be shared with the bracket."));
        }
    }

    private void StartDeadlineRefresh()
    {
        StopDeadlineRefresh(resetTrigger: false);
        if(!_hasLoaded || !GetDeadlineUtc().HasValue)
            return;

        _deadlineRefreshCancellation = new CancellationTokenSource();
        _deadlineRefreshTask = RefreshDeadlineAsync(_deadlineRefreshCancellation.Token);
    }

    private void StopDeadlineRefresh(bool resetTrigger = true)
    {
        _deadlineRefreshCancellation?.Cancel();
        _deadlineRefreshCancellation?.Dispose();
        _deadlineRefreshCancellation = null;
        _deadlineRefreshTask = null;
        if(resetTrigger)
            _deadlineRefreshTriggeredFor = null;
    }

    private async Task RefreshDeadlineAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        try
        {
            while(await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(async () =>
                {
                    if(cancellationToken.IsCancellationRequested)
                        return;

                    var deadline = GetDeadlineUtc();
                    if(!deadline.HasValue)
                    {
                        StopDeadlineRefresh();
                        StateHasChanged();
                        return;
                    }

                    if(deadline.Value.ToUniversalTime() <= DateTime.UtcNow &&
                       !_isLoading &&
                       !_isSubmitting &&
                       _deadlineRefreshTriggeredFor != deadline)
                    {
                        _deadlineRefreshTriggeredFor = deadline;
                        await RefreshAsync(Match.Id);
                        return;
                    }

                    StateHasChanged();
                });
            }
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
        catch(ObjectDisposedException) when(cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task RetryAsync() => await RefreshAsync(Match.Id);

    private async Task ConfirmEndedAsync()
    {
        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.ConfirmMatchEndedAsync(matchId),
            "Your match-end confirmation was saved.",
            state => state.CanConfirmEnded,
            MatchMutationAction.ConfirmEnded);
    }

    private async Task SubmitScoreAsync()
    {
        if(_actionState?.CanSubmitScore != true ||
           _participant1Score is not { } participant1Score ||
           _participant2Score is not { } participant2Score)
            return;

        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.SubmitMatchScoreAsync(
                matchId,
                new SubmitMatchScoreDTO
                {
                    Participant1Score = participant1Score,
                    Participant2Score = participant2Score
                }),
            "Your score report was saved.",
            state => state.CanSubmitScore,
            MatchMutationAction.SubmitScore);
    }

    private async Task ForfeitOwnSideAsync()
    {
        if(_actionState?.AuthorizedParticipant is not MatchParticipantSide side)
            return;

        if(!_hasFreshActionState || _actionState?.CanForfeit != true ||
           _forfeitConfirmationRequested == false)
            return;

        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.ForfeitMatchAsync(matchId, new ForfeitMatchDTO { Participant = side }),
            "The forfeit was saved.",
            state => state.CanForfeit,
            MatchMutationAction.Forfeit);
        _forfeitConfirmationRequested = false;
    }

    private Task RequestAdminForfeitAsync(MatchParticipantSide side)
    {
        if(!_hasFreshActionState || _actionState?.CanForceForfeit != true || _isSubmitting || _isLoading)
            return Task.CompletedTask;

        _adminForfeitConfirmationSide = side;
        return InvokeAsync(StateHasChanged);
    }

    private async Task ForfeitSideAsAdminAsync(MatchParticipantSide side)
    {
        if(_adminForfeitConfirmationSide != side || _actionState?.CanForceForfeit != true)
            return;

        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.ForfeitMatchAsync(matchId, new ForfeitMatchDTO { Participant = side }),
            "The administrator forfeit was saved.",
            state => state.CanForceForfeit,
            MatchMutationAction.ForceForfeit);
        _adminForfeitConfirmationSide = null;
    }

    private async Task ResolveAsync()
    {
        if(_actionState?.CanResolve != true ||
           _participant1Score is not { } participant1Score ||
           _participant2Score is not { } participant2Score)
            return;

        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.ResolveMatchAsync(
                matchId,
                new ResolveMatchDTO
                {
                    Participant1Score = participant1Score,
                    Participant2Score = participant2Score
                }),
            "The match was resolved and the result is official.",
            state => state.CanResolve,
            MatchMutationAction.Resolve);
    }

    private async Task ReverseAsync()
    {
        if(_actionState?.CanReverse != true)
            return;

        var matchId = Match.Id;
        await RunMutationAsync(
            () => TournamentService.ReverseMatchAsync(matchId),
            "The match result was reversed.",
            state => state.CanReverse,
            MatchMutationAction.Reverse);
    }

    private async Task RunMutationAsync(
        Func<Task<Match>> mutation,
        string successMessage,
        Func<MatchActionStateDTO, bool> capability,
        MatchMutationAction action)
    {
        if(_isSubmitting || _isLoading || !_hasLoaded || !_hasFreshActionState || _actionState is null ||
           !capability(_actionState))
            return;

        var expectedMatchId = Match.Id;
        var expectedRefreshGeneration = _refreshGeneration;
        _isSubmitting = true;
        _isLoading = true;
        _errorMessage = null;
        _forfeitConfirmationRequested = false;
        _adminForfeitConfirmationSide = null;
        _reverseConfirmationRequested = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var latestState = await TournamentService.GetMatchActionStateAsync(expectedMatchId);
            if(expectedMatchId != Match.Id || expectedRefreshGeneration != _refreshGeneration)
                return;

            Match = latestState.Match;
            _freshMatchProjection = Match;
            _actionState = latestState;
            _hasFreshActionState = true;
            await NotifyMatchRefreshedAsync();
            if(!capability(latestState))
            {
                _errorMessage = GetBlockedReason(latestState, action);
                return;
            }

            var result = await mutation();
            if(expectedMatchId != Match.Id || expectedRefreshGeneration != _refreshGeneration)
                return;

            Match = result;
            _freshMatchProjection = Match;
            await NotifyMatchRefreshedAsync();
            var refreshed = await RefreshAsync(expectedMatchId);
            if(refreshed)
            {
                if(_requiresAuthentication)
                    ToastService.ShowWarning("Saved, but only the public match state could be refreshed. Sign in to manage this match.");
                else
                    ToastService.ShowSuccess(successMessage);

                try
                {
                    await OnDataReload.InvokeAsync(Match);
                }
                catch(Exception exception)
                {
                    // The command and protected match refresh succeeded. Keep that fresh
                    // projection visible even when the surrounding tournament reload fails.
                    _errorMessage = GetErrorMessage(
                        exception,
                        "Saved, but the tournament display could not be refreshed. Retry to verify the bracket.");
                    ToastService.ShowWarning(_errorMessage);
                }
            }
            else
            {
                ToastService.ShowWarning("Saved, but the latest match state could not be refreshed. Retry to verify the result.");
            }
        }
        catch(Exception exception)
        {
            if(expectedMatchId == Match.Id)
            {
                _hasFreshActionState = false;
                _errorMessage = GetErrorMessage(exception, "The match action could not be saved. Refresh and try again.");
                ToastService.ShowError(_errorMessage);
            }
        }
        finally
        {
            if(expectedMatchId == Match.Id)
                _isSubmitting = false;
            if(expectedMatchId == Match.Id)
                _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    internal static string GetBlockedReason(MatchActionStateDTO state, MatchMutationAction action)
    {
        return action switch
        {
            MatchMutationAction.ForceForfeit => GetAdminBlockedReason(state.ForceForfeitBlockedReason),
            MatchMutationAction.Resolve => GetAdminBlockedReason(state.ResolveBlockedReason),
            MatchMutationAction.Reverse => GetAdminBlockedReason(state.ReverseBlockedReason),
            MatchMutationAction.ConfirmEnded => "Match-end confirmation is no longer available in the authoritative match state.",
            MatchMutationAction.SubmitScore => "Score submission is no longer available in the authoritative match state.",
            MatchMutationAction.Forfeit => "Forfeiting this match is no longer available for your side in the authoritative state.",
            _ => "The match changed while you were working. Refresh the authoritative state and try again."
        };
    }

    private static string GetErrorMessage(Exception exception, string fallback)
    {
        if(exception is ApiException apiException)
        {
            if(apiException.StatusCode == HttpStatusCode.Unauthorized)
                return "Sign in to manage this match.";
            if(apiException.StatusCode == HttpStatusCode.Forbidden)
            {
                return "You are not authorized to perform this match action.";
            }
            if(apiException.StatusCode == HttpStatusCode.Conflict)
            {
                var apiError = apiException.GetApiError();
                if(apiError?.Code == "match_reversal_blocked")
                    return "This result cannot be reversed because a linked downstream match has already been played or resolved.";
                if(apiError?.Code == "downstream_graph_too_large")
                    return "This result cannot be reversed until the linked bracket is reviewed by an administrator.";
                if(apiError?.Code == "match_requires_admin_resolution")
                    return "This match already requires administrator resolution.";
                return apiError?.Message
                    ?? "The match changed while you were working. Refresh the authoritative state and try again.";
            }
            if(!string.IsNullOrWhiteSpace(apiException.Content))
                return apiException.GetApiError()?.Message ?? apiException.Content!;
        }

        if(exception is InvalidOperationException && !string.IsNullOrWhiteSpace(exception.Message))
            return exception.Message;

        return fallback;
    }

    public async ValueTask DisposeAsync()
    {
        var refreshTask = _deadlineRefreshTask;
        StopDeadlineRefresh();

        if(refreshTask is null)
            return;

        try
        {
            await refreshTask;
        }
        catch(OperationCanceledException)
        {
        }
        catch(ObjectDisposedException)
        {
        }
    }
}
