using Mercurius.LAN.Web.DTOs.Users;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantComponent
{
    [Parameter] public ParticipantViewModel? Participant { get; set; }
    [Parameter] public string EmptyLabel { get; set; } = "TBD";
    [Parameter] public bool ShowIdentityHeader { get; set; } = true;

    private static string GetUserLabel(PublicUserDTO user)
    {
        if(!string.IsNullOrWhiteSpace(user.Username))
            return user.Username.Trim();

        if(!string.IsNullOrWhiteSpace(user.DisplayName))
            return user.DisplayName.Trim();

        var fullName = GetFullName(user);
        return string.IsNullOrWhiteSpace(fullName) ? "Participant" : fullName;
    }

    private static bool HasPublicUsername(PublicUserDTO user)
    {
        return !string.IsNullOrWhiteSpace(user.Username);
    }

    private static string GetUserProfileHref(PublicUserDTO user)
    {
        return $"/users/{Uri.EscapeDataString(user.Username!.Trim())}";
    }

    private static string GetTeamProfileHref(Models.Participants.Team team)
    {
        return $"/teams/{Uri.EscapeDataString(team.Name.Trim())}";
    }

    private static bool HasTeamName(Models.Participants.Team team) =>
        !string.IsNullOrWhiteSpace(team.Name);

    private static string GetFullName(PublicUserDTO user)
    {
        return string.Join(" ", new[] { user.Firstname, user.Lastname }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));
    }
}
