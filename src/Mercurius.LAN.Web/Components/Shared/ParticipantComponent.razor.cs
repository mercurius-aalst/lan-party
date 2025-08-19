using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantComponent
{
    [Parameter] public Participant Participant { get; set; } = null!;
    [Parameter] public string EmptyLabel { get; set; } = "TBD";
}