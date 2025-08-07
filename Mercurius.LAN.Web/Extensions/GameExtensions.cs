using Mercurius.LAN.Web.Models;
using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.Extensions;

public static class GameExtensions
{
    public static string GetStatusLabel(this string status)
    {
        return status.ToLower() switch
        {
            "scheduled" => "Open",
            "inprogress" => "Bezig",
            "completed" => "Afgerond",
            "canceled" => "Geannuleerd",
            _ => status
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

    public static string GetStatusClass(this string status)
    {
        return status?.ToLower() switch
        {
            "scheduled" => "status-scheduled",
            "inprogress" => "status-inprogress",
            "completed" => "status-completed",
            "canceled" => "status-canceled",
            _ => string.Empty
        };
    }
}