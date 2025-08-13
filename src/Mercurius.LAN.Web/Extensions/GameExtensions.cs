using Mercurius.LAN.Web.Models;
using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.Extensions;

public static class GameExtensions
{
    public static string GetStatusLabel(this GameStatus status)
    {
        return status switch
        {
            GameStatus.Scheduled => "Open",
            GameStatus.InProgress => "Ongoing",
            GameStatus.Completed => "Finished",
            GameStatus.Canceled => "Cancelled",
            _ => status.ToString()
        };
    }

    public static string GetLabel(this BracketType bracketType)
    {
        return bracketType switch
        {
            BracketType.SingleElimination => "Single Elimination",
            BracketType.DoubleElimination => "Double Elimination",
            BracketType.RoundRobin => "Round Robin",
            BracketType.Swiss => "Swiss",
            _ => bracketType.ToString()
        };
    }

    public static string GetLabel(this GameFormat gameFormat)
    {
        return gameFormat switch
        {
            GameFormat.BestOf1 => "Best of 1",
            GameFormat.BestOf3 => "Best of 3",
            GameFormat.BestOf5 => "Best of 5",
            _ => gameFormat.ToString()
        };
    }

    public static string GetStatusClass(this GameStatus status)
    {
        return status switch
        {
            GameStatus.Scheduled => "status-scheduled",
            GameStatus.InProgress => "status-inprogress",
            GameStatus.Completed => "status-completed",
            GameStatus.Canceled => "status-canceled",
            _ => string.Empty
        };
    }
}