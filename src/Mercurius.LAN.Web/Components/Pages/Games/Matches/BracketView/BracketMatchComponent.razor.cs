using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class BracketMatchComponent
{
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public (int left, int y)? Position { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
    [Parameter] public string ExtraCssClasses { get; set; } = string.Empty;

    private Guid? Participant1Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant1Id : Match.UserParticipant1Id;
    private Guid? Participant2Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant2Id : Match.UserParticipant2Id;
    private string Participant1Name => RetrieveParticipantName(Participant1Id, Match.Participant1IsBYE);
    private string Participant2Name => RetrieveParticipantName(Participant2Id, Match.Participant2IsBYE);
    private bool _showDialog;

    private string RetrieveParticipantName(Guid? participantId, bool isBye)
    {
        if(isBye)
            return "BYE";

        return ResolveParticipant(participantId)?.DisplayName ?? "TBD";
    }

    private ParticipantViewModel? ResolveParticipant(Guid? participantId)
    {
        if(participantId is null)
            return null;

        return Match.ParticipationMode switch
        {
            ParticipationMode.Individual => Game.Users.Where(user => user.Id == participantId.Value).Select(ParticipantViewModel.FromUser).FirstOrDefault(),
            ParticipationMode.Team => Game.Teams.Where(team => team.Id == participantId.Value).Select(ParticipantViewModel.FromTeam).FirstOrDefault(),
            _ => null
        };
    }

    private void DisplayDetails()
    {
        _showDialog = true;
    }

    private async Task CloseDetailsDialogAsync()
    {
        _showDialog = false;
        await OnDataReload.InvokeAsync();
    }
}
