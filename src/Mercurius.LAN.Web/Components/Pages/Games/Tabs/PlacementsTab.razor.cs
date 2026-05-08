using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class PlacementsTab
{
    [Parameter] public IEnumerable<Placement> Placements { get; set; } = Enumerable.Empty<Placement>();
    [Parameter] public ParticipationMode ParticipationMode { get; set; }

    private IEnumerable<string> GetParticipantNames(Placement placement)
    {
        return ParticipationMode switch
        {
            ParticipationMode.Individual => placement.Users.Select(GetUserLabel),
            ParticipationMode.Team => placement.Teams.Select(team => team.Name),
            _ => Enumerable.Empty<string>()
        };
    }

    private static string GetUserLabel(UserDTO user)
    {
        return user.Username ?? user.DisplayName;
    }

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
