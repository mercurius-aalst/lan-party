using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class PlacementsTab
{
    [Parameter]
    public IEnumerable<Placement> Placements { get; set; } = Enumerable.Empty<Placement>();

    private string GetOrdinalSuffix(int number)
    {
        if(number % 100 >= 11 && number % 100 <= 13)
        {
            return number + "th";
        }

        int num = number % 10;
        string suffix = num switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };

        return number + suffix;
    }
}