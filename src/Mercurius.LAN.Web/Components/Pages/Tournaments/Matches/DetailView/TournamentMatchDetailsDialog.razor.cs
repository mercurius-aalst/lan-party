using System.Net;
using System.Text.Json;
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
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
    [Parameter] public string Participant2Name { get; set; } = null!;
    [Parameter] public string Participant1Name { get; set; } = null!;
    [Parameter] public ParticipantViewModel? Participant1 { get; set; }
    [Parameter] public ParticipantViewModel? Participant2 { get; set; }

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

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
    private bool _forfeitConfirmationRequested;
    private string? _errorMessage;
    private int _participant1Score;
    private int _participant2Score;
    private CancellationTokenSource? _deadlineRefreshCancellation;
    private DateTime? _deadlineRefreshTriggeredFor;
    private Task? _deadlineRefreshTask;

    protected override async Task OnParametersSetAsync()
    {
        _participantLookup = TournamentParticipantLookup.FromTournament(Tournament);
        if(_loadedMatchId == Match.Id)
            return;

        _loadedMatchId = Match.Id;
        _hasLoaded = false;
        _actionState = null;
        _errorMessage = null;
        _requiresAuthentication = false;
        _forfeitConfirmationRequested = false;
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
        MatchLifecycleState.AwaitingScore => "Both sides have confirmed the end. The assigned side may submit the score.",
        MatchLifecycleState.ScoreConfirmation => "The first score report is saved. The opponent has five minutes to agree or report a correction.",
        MatchLifecycleState.Disputed => "The reports do not match. Each side has one correction opportunity before administrator resolution.",
        MatchLifecycleState.AdminResolutionRequired => "The correction window expired. An assigned tournament administrator must resolve this result.",
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

    private bool ShowPrivateReports => _actionState is not null;

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

    private string ActionSubjectLabel => Match.ParticipationMode == ParticipationMode.Team
        ? "Your captain actions"
        : "Your player actions";

    private static string FormatReport(int? participant1Score, int? participant2Score) =>
        participant1Score.HasValue && participant2Score.HasValue
            ? $"{participant1Score}-{participant2Score}"
            : "Not submitted";

    private async Task<bool> RefreshAsync(Guid expectedMatchId)
    {
        var generation = ++_refreshGeneration;
        _isLoading = true;
        _errorMessage = null;
        _requiresAuthentication = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var state = await TournamentService.GetMatchActionStateAsync(expectedMatchId);
            if(generation != _refreshGeneration || Match.Id != expectedMatchId)
                return false;

            Match = state.Match;
            _actionState = state;
            _participant1Score = state.Participant1ReportedScore1 ?? Match.Participant1Score ?? 0;
            _participant2Score = state.Participant2ReportedScore1 ?? Match.Participant2Score ?? 0;
            _hasLoaded = true;
            StartDeadlineRefresh();
            return true;
        }
        catch(ApiException exception) when(exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            if(generation != _refreshGeneration || Match.Id != expectedMatchId)
                return false;

            _actionState = null;
            _requiresAuthentication = true;
            _hasLoaded = true;
            StopDeadlineRefresh();
            return true;
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound)
        {
            if(generation == _refreshGeneration && Match.Id == expectedMatchId)
            {
                _errorMessage = "This match is no longer available.";
                _hasLoaded = true;
            }
            return false;
        }
        catch(Exception exception)
        {
            if(generation == _refreshGeneration && Match.Id == expectedMatchId)
            {
                _errorMessage = GetErrorMessage(exception, "The authoritative match state is unavailable.");
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

    private void StartDeadlineRefresh()
    {
        StopDeadlineRefresh();
        if(!_hasLoaded || !GetDeadlineUtc().HasValue)
            return;

        _deadlineRefreshCancellation = new CancellationTokenSource();
        _deadlineRefreshTask = RefreshDeadlineAsync(_deadlineRefreshCancellation.Token);
    }

    private void StopDeadlineRefresh()
    {
        _deadlineRefreshCancellation?.Cancel();
        _deadlineRefreshCancellation?.Dispose();
        _deadlineRefreshCancellation = null;
        _deadlineRefreshTask = null;
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
        await RunMutationAsync(
            () => TournamentService.ConfirmMatchEndedAsync(Match.Id),
            "Your match-end confirmation was saved.");
    }

    private async Task SubmitScoreAsync()
    {
        if(_actionState?.CanSubmitScore != true)
            return;

        await RunMutationAsync(
            () => TournamentService.SubmitMatchScoreAsync(
                Match.Id,
                new SubmitMatchScoreDTO
                {
                    Participant1Score = _participant1Score,
                    Participant2Score = _participant2Score
                }),
            "Your score report was saved.");
    }

    private async Task ForfeitOwnSideAsync()
    {
        if(_actionState?.AuthorizedParticipant is not MatchParticipantSide side)
            return;

        await RunMutationAsync(
            () => TournamentService.ForfeitMatchAsync(Match.Id, new ForfeitMatchDTO { Participant = side }),
            "The forfeit was saved.");
        _forfeitConfirmationRequested = false;
    }

    private async Task ForfeitSideAsAdminAsync(MatchParticipantSide side)
    {
        await RunMutationAsync(
            () => TournamentService.ForfeitMatchAsync(Match.Id, new ForfeitMatchDTO { Participant = side }),
            "The administrator forfeit was saved.");
    }

    private async Task ResolveAsync()
    {
        await RunMutationAsync(
            () => TournamentService.ResolveMatchAsync(
                Match.Id,
                new ResolveMatchDTO
                {
                    Participant1Score = _participant1Score,
                    Participant2Score = _participant2Score
                }),
            "The match was resolved and the result is official.");
    }

    private async Task ReverseAsync()
    {
        await RunMutationAsync(
            () => TournamentService.ReverseMatchAsync(Match.Id),
            "The match result was reversed.");
    }

    private async Task RunMutationAsync(Func<Task<Match>> mutation, string successMessage)
    {
        if(_isSubmitting || _isLoading || !_hasLoaded)
            return;

        var expectedMatchId = Match.Id;
        var expectedRefreshGeneration = _refreshGeneration;
        _isSubmitting = true;
        _errorMessage = null;
        _forfeitConfirmationRequested = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await mutation();
            if(expectedMatchId != Match.Id || expectedRefreshGeneration != _refreshGeneration)
                return;

            Match = result;
            var refreshed = await RefreshAsync(expectedMatchId);
            if(refreshed && !_requiresAuthentication)
            {
                ToastService.ShowSuccess(successMessage);
                await OnDataReload.InvokeAsync();
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
                _errorMessage = GetErrorMessage(exception, "The match action could not be saved. Refresh and try again.");
                ToastService.ShowError(_errorMessage);
            }
        }
        finally
        {
            if(expectedMatchId == Match.Id)
                _isSubmitting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string GetErrorMessage(Exception exception, string fallback)
    {
        if(exception is ApiException apiException)
        {
            if(apiException.StatusCode == HttpStatusCode.Unauthorized)
                return "Sign in to manage this match.";
            if(apiException.StatusCode == HttpStatusCode.Forbidden)
                return "You are not authorized to perform this match action.";
            if(apiException.StatusCode == HttpStatusCode.Conflict)
                return GetServerMessage(apiException.Content)
                    ?? "The match changed while you were working. Refresh the authoritative state and try again.";
            if(!string.IsNullOrWhiteSpace(apiException.Content))
                return GetServerMessage(apiException.Content) ?? apiException.Content!;
        }

        if(exception is InvalidOperationException && !string.IsNullOrWhiteSpace(exception.Message))
            return exception.Message;

        return fallback;
    }

    private static string? GetServerMessage(string? content)
    {
        if(string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            if(document.RootElement.ValueKind == JsonValueKind.Object &&
               document.RootElement.TryGetProperty("message", out var message) &&
               message.ValueKind == JsonValueKind.String)
                return message.GetString();

            if(document.RootElement.ValueKind == JsonValueKind.String)
                return document.RootElement.GetString();
        }
        catch(JsonException)
        {
            // Refit can expose a plain-text body when a proxy or legacy endpoint responds.
        }

        return content.Trim().Trim('"');
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
