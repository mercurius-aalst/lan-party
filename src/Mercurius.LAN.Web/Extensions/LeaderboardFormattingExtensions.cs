using System.Globalization;
using Mercurius.LAN.Web.Models.Tournaments;

namespace Mercurius.LAN.Web.Extensions;

public static class LeaderboardFormattingExtensions
{
    public static string FormatLeaderboardScore(decimal score) =>
        score.ToString("0.######", CultureInfo.InvariantCulture);

    public static string FormatLeaderboardDuration(long durationMilliseconds)
    {
        var duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss\.fff", CultureInfo.InvariantCulture)
            : duration.ToString(@"m\:ss\.fff", CultureInfo.InvariantCulture);
    }

    public static string FormatLeaderboardValue(
        LeaderboardRankingMetric metric,
        decimal? score,
        long? durationMilliseconds)
    {
        if(metric == LeaderboardRankingMetric.HighestScore)
            return score.HasValue ? FormatLeaderboardScore(score.Value) : string.Empty;

        return durationMilliseconds.HasValue
            ? FormatLeaderboardDuration(durationMilliseconds.Value)
            : string.Empty;
    }

    public static LeaderboardRankingMetric ResolveLeaderboardMetric(this Tournament tournament) =>
        tournament.LeaderboardRankingMetric ?? LeaderboardRankingMetric.HighestScore;

    public static bool IsLeaderboard(this Tournament tournament) =>
        tournament.BracketType == BracketType.Leaderboard;
}
