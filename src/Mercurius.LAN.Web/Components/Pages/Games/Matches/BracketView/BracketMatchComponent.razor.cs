// Moved @code block from BracketMatchComponent.razor
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class BracketMatchComponent
{
    [Parameter]
    public Match Match { get; set; } = null!;
    [Parameter] public (int left, int y)? Position { get; set; }
    [Parameter]
    public IEnumerable<Participant> Participants { get; set; } = Enumerable.Empty<Participant>();
    [Parameter]
    public EventCallback OnDataReload { get; set; }
    [Parameter]
    public string ExtraCssClasses { get; set; } = string.Empty;

    private string Participant1Name => RetrieveParticipantName(Match.Participant1Id, Match.Participant1IsBYE);
    private string Participant2Name => RetrieveParticipantName(Match.Participant2Id, Match.Participant2IsBYE);
    private bool _showDialog = false;

    private string RetrieveParticipantName(int? participantId, bool isBye)
    {
        if(isBye)
            return "BYE";
        if(participantId is null || Participants == null)
            return "TBD";

        var participant = Participants.FirstOrDefault(p => p.Id == participantId.Value);
        return participant switch
        {
            Player player => player.Username,
            Team team => team.Name,
            _ => "TBD"
        };
    }

    private void DisplayDetails()
    {
        _showDialog = true;
    }

    private async Task CloseDetailsDialogAsync()
    {
        _showDialog = false;
        await OnDataReload.InvokeAsync(); // Notify parent to reload data
    }
}