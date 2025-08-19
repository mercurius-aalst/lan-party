using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantCardComponent
{
    private bool _showDeleteIcon = false;

    [Parameter] public Participant Participant { get; set; } = null!;
    [Parameter] public EventCallback<Participant> OnParticipantSelected { get; set; }
    [Parameter] public EventCallback<Participant> OnParticipantDeleted { get; set; }
    [Parameter] public bool AllowDeleteFunction { get; set; } = false;

    private void ShowParticipantPopup(Participant participant)
    {
        OnParticipantSelected.InvokeAsync(participant);
    }
}