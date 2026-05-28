using Mercurius.LAN.Web.DTOs.Users;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantComponent
{
    [Parameter] public ParticipantViewModel? Participant { get; set; }
    [Parameter] public string EmptyLabel { get; set; } = "TBD";
    [Parameter] public bool ShowIdentityHeader { get; set; } = true;

    private static string GetUserLabel(UserDTO user)
    {
        return user.Username ?? user.DisplayName;
    }

    private static string RenderValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Not provided" : value;
    }
}
