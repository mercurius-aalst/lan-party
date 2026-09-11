using Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class TournamentParticipantDialogStateTests
{
    [Fact]
    public async Task TeamMemberDetailsTemporarilyOwnTheOnlyActiveDialogAndReturnToParticipant()
    {
        var captain = new PublicUserDTO { Id = Guid.NewGuid(), Username = "captain" };
        var member = new PublicUserDTO { Id = Guid.NewGuid(), Username = "member" };
        var participant = ParticipantViewModel.FromTeam(new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team Focus",
            CaptainUserId = captain.Id,
            Members = [captain, member]
        });
        var tab = new TournamentParticipantsTab();

        tab.DisplayParticipantPopup(participant);
        Assert.True(TournamentParticipantsTab.ShouldRenderParticipantDialog(false, participant, null));

        tab.DisplayUserPopup(member);
        var userDetailsState = tab.GetParticipantDialogState();
        Assert.Same(participant, userDetailsState.Participant);
        Assert.Same(member, userDetailsState.User);
        Assert.False(TournamentParticipantsTab.ShouldRenderParticipantDialog(
            false,
            userDetailsState.Participant,
            userDetailsState.User));

        tab.HideUserInfoPopup();
        var restoredState = tab.GetParticipantDialogState();
        Assert.Same(participant, restoredState.Participant);
        Assert.Null(restoredState.User);
        Assert.True(restoredState.RestoreParticipantFocus);
        Assert.True(TournamentParticipantsTab.ShouldRenderParticipantDialog(
            false,
            restoredState.Participant,
            restoredState.User));

        await tab.HandleParticipantDialogKeyDown(new KeyboardEventArgs { Key = "Escape" });
        var closedState = tab.GetParticipantDialogState();
        Assert.Null(closedState.Participant);
        Assert.Null(closedState.User);
        Assert.False(closedState.RestoreParticipantFocus);
    }

    [Fact]
    public void ParticipantDialogNeverRendersForPopupOnlyOrWhileUserDetailsAreOpen()
    {
        var participant = ParticipantViewModel.FromTeam(new Team
        {
            Id = Guid.NewGuid(),
            Name = "Team Focus",
            CaptainUserId = Guid.NewGuid()
        });
        var user = new PublicUserDTO { Id = Guid.NewGuid(), Username = "member" };

        Assert.False(TournamentParticipantsTab.ShouldRenderParticipantDialog(true, participant, null));
        Assert.False(TournamentParticipantsTab.ShouldRenderParticipantDialog(false, participant, user));
        Assert.False(TournamentParticipantsTab.ShouldRenderParticipantDialog(false, null, null));
    }
}
