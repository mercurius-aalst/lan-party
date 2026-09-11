using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Extensions;
using MatchLifecycleState = Mercurius.LAN.Web.DTOs.Matches.MatchLifecycleState;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class PublicProfileMatchSummaries
{
    [Parameter] public PublicProfileMatchSummariesDTO? Summaries { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool HasError { get; set; }
    [Parameter] public EventCallback OnRetry { get; set; }

    private static string BuildTournamentHref(PublicProfileMatchSummaryDTO summary) =>
        $"/tournaments/{summary.TournamentId}";

    private string GetOpponentLabel(PublicProfileMatchSummaryDTO summary) =>
        summary.OpponentIsTbd || string.IsNullOrWhiteSpace(summary.OpponentDisplayName)
            ? Localization["shared.tbd"]
            : summary.OpponentDisplayName;

    private string GetResultLabel(PublicProfileMatchSummaryDTO summary)
    {
        if(summary.LifecycleState == MatchLifecycleState.Forfeited)
        {
            return summary.ParticipantScore.HasValue && summary.OpponentScore.HasValue
                ? Localization.Get("match.forfeitScore", summary.ParticipantScore, summary.OpponentScore)
                : Localization["match.forfeitRecorded"];
        }

        if(summary.ParticipantScore.HasValue && summary.OpponentScore.HasValue)
            return Localization.Get("match.score", summary.ParticipantScore, summary.OpponentScore);

        return summary.LifecycleState switch
        {
            MatchLifecycleState.Completed => Localization["status.completed"],
            _ => Localization["match.resultRecorded"]
        };
    }

    private string GetUpcomingStateLabel(PublicProfileMatchSummaryDTO summary)
    {
        if(summary.LifecycleState == MatchLifecycleState.AwaitingEndedConfirmation &&
           summary.EstimatedStartTime is { } estimatedStart &&
           estimatedStart.ToUniversalTime() <= DateTime.UtcNow)
            return Localization["match.awaitingStartEstimatePassed"];

        return summary.LifecycleState == MatchLifecycleState.AwaitingEndedConfirmation
            ? Localization["match.scheduled"]
            : Localization["match.upcoming"];
    }

    private string GetRoundLabel(PublicProfileMatchSummaryDTO summary)
    {
        var bracket = summary.IsLowerBracketMatch ? Localization["match.lowerBracket"] : Localization["match.upperBracket"];
        return Localization.Get("match.round", bracket, summary.RoundNumber, summary.MatchNumber);
    }

    private string GetPreviousTimeLabel(PublicProfileMatchSummaryDTO summary)
    {
        var completedAt = summary.CompletedAtUtc ?? summary.StartedAtUtc;
        return completedAt.HasValue
            ? Localization.Get("match.played", FormatLocal(completedAt.Value))
            : Localization["match.playedDateUnavailable"];
    }

    private string GetUpcomingTimeLabel(PublicProfileMatchSummaryDTO summary)
    {
        if(summary.EstimatedStartTime is { } estimatedStart)
            return Localization.Get("match.estimated", FormatLocal(estimatedStart));

        if(summary.ScheduledStartTime is { } scheduledStart)
            return Localization.Get("match.scheduledAt", FormatLocal(scheduledStart));

        return Localization["match.timeToBeConfirmed"];
    }

    private static string? GetTimeAttribute(DateTime? value) =>
        value?.ToUtcIsoString();

    private string FormatLocal(DateTime value) => Localization.FormatDateTime(value.ToLocalDisplayTime());

    private Task RetryAsync() => OnRetry.InvokeAsync();
}
