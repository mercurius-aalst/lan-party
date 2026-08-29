using Mercurius.LAN.Web.DTOs.PublicProfiles;
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

    private static string GetOpponentLabel(PublicProfileMatchSummaryDTO summary) =>
        summary.OpponentIsTbd || string.IsNullOrWhiteSpace(summary.OpponentDisplayName)
            ? "Opponent TBD"
            : summary.OpponentDisplayName;

    private static string GetResultLabel(PublicProfileMatchSummaryDTO summary)
    {
        if(summary.LifecycleState == MatchLifecycleState.Forfeited)
        {
            return summary.ParticipantScore.HasValue && summary.OpponentScore.HasValue
                ? $"Forfeit · participant {summary.ParticipantScore} - {summary.OpponentScore} opponent"
                : "Forfeit recorded";
        }

        if(summary.ParticipantScore.HasValue && summary.OpponentScore.HasValue)
            return $"Score · participant {summary.ParticipantScore} - {summary.OpponentScore} opponent";

        return summary.LifecycleState switch
        {
            MatchLifecycleState.Completed => "Completed",
            _ => "Result recorded"
        };
    }

    private static string GetUpcomingStateLabel(PublicProfileMatchSummaryDTO summary) =>
        summary.LifecycleState == MatchLifecycleState.AwaitingEndedConfirmation
            ? "Scheduled match"
            : "Upcoming match";

    private static string GetRoundLabel(PublicProfileMatchSummaryDTO summary)
    {
        var bracket = summary.IsLowerBracketMatch ? "Lower bracket" : "Upper bracket";
        return $"{bracket}, round {summary.RoundNumber}, match {summary.MatchNumber}";
    }

    private static string GetPreviousTimeLabel(PublicProfileMatchSummaryDTO summary)
    {
        var completedAt = summary.CompletedAtUtc ?? summary.StartedAtUtc;
        return completedAt.HasValue
            ? $"Played {FormatUtc(completedAt.Value)}"
            : "Played date unavailable";
    }

    private static string GetUpcomingTimeLabel(PublicProfileMatchSummaryDTO summary)
    {
        if(summary.EstimatedStartTime is { } estimatedStart)
            return $"Estimated {FormatUtc(estimatedStart)}";

        if(summary.ScheduledStartTime is { } scheduledStart)
            return $"Scheduled {FormatUtc(scheduledStart)}";

        return "Time to be confirmed";
    }

    private static string? GetTimeAttribute(DateTime? value) =>
        value.HasValue ? value.Value.ToUniversalTime().ToString("O") : null;

    private static string FormatUtc(DateTime value) =>
        $"{value.ToUniversalTime():dd MMM yyyy, HH:mm} UTC";

    private Task RetryAsync() => OnRetry.InvokeAsync();
}
