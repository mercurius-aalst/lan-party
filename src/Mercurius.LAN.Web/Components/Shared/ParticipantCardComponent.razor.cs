using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class ParticipantCardComponent
{
    private bool _showDeleteIcon;

    [Parameter] public ParticipantViewModel Participant { get; set; } = null!;
    [Parameter] public EventCallback<ParticipantViewModel> OnParticipantSelected { get; set; }
    [Parameter] public EventCallback<ParticipantViewModel> OnParticipantDeleted { get; set; }
    [Parameter] public bool AllowDeleteFunction { get; set; }

    private string? TeamLogoUrl => Participant.Team?.LogoUrl;

    private bool IsTeamParticipant => Participant.Team is not null;

    private Task ShowParticipantPopup(ParticipantViewModel participant)
    {
        return OnParticipantSelected.InvokeAsync(participant);
    }

    private Task HandleKeyDown(KeyboardEventArgs args) =>
        args.Key is "Enter" or " "
            ? ShowParticipantPopup(Participant)
            : Task.CompletedTask;

    private string GetParticipantInitial() =>
        string.IsNullOrWhiteSpace(Participant.DisplayName)
            ? "?"
            : Participant.DisplayName.Trim()[0].ToString().ToUpperInvariant();
}
