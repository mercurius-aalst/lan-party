using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.BracketView;

public partial class TournamentBracketMatchComponent
{
    [Parameter] public Match Match { get; set; } = null!;
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public (int left, int y)? Position { get; set; }
    [Parameter] public EventCallback OnDataReload { get; set; }
    [Parameter] public string ExtraCssClasses { get; set; } = string.Empty;

    private Guid? Participant1Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant1Id : Match.UserParticipant1Id;
    private Guid? Participant2Id => Match.ParticipationMode == ParticipationMode.Team ? Match.TeamParticipant2Id : Match.UserParticipant2Id;
    private string Participant1Name => RetrieveParticipantName(Participant1Id, Match.Participant1IsBYE);
    private string Participant2Name => RetrieveParticipantName(Participant2Id, Match.Participant2IsBYE);
    private bool _showDialog;
    private TournamentParticipantLookup _participantLookup = TournamentParticipantLookup.Empty;

    protected override void OnParametersSet()
    {
        _participantLookup = TournamentParticipantLookup.FromTournament(Tournament);
    }

    private string RetrieveParticipantName(Guid? participantId, bool isBye)
    {
        if(isBye)
            return "BYE";

        return _participantLookup.ResolveName(Match.ParticipationMode, participantId);
    }

    private ParticipantViewModel? ResolveParticipant(Guid? participantId)
    {
        return _participantLookup.Resolve(Match.ParticipationMode, participantId);
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
