using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantCardComponent
{
    private bool _showDeleteIcon;

    [Parameter] public ParticipantViewModel Participant { get; set; } = null!;
    [Parameter] public EventCallback<ParticipantViewModel> OnParticipantSelected { get; set; }
    [Parameter] public EventCallback<ParticipantViewModel> OnParticipantDeleted { get; set; }
    [Parameter] public bool AllowDeleteFunction { get; set; }

    private void ShowParticipantPopup(ParticipantViewModel participant)
    {
        OnParticipantSelected.InvokeAsync(participant);
    }
}
