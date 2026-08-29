using Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class TournamentRegistrationWorkflowStateTests
{
    [Theory]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    [InlineData(true, true, false)]
    public void SavedMutationRefreshFailureIsReportedOnlyForTheCurrentRoute(
        bool currentUserStateLoaded,
        bool isCurrentRoute,
        bool expected)
    {
        Assert.Equal(
            expected,
            TournamentParticipantsTab.ShouldReportSavedMutationRefreshFailure(
                currentUserStateLoaded,
                isCurrentRoute));
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, false, false)]
    public void DirtyRosterDraftIsKeptWhenItsTeamIsNoLongerSelected(
        bool hasDraftTeam,
        bool selectedTeamIsSame,
        bool expected)
    {
        Guid? draftTeamId = hasDraftTeam
            ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")
            : null;
        Guid? selectedTeamId = hasDraftTeam && selectedTeamIsSame
            ? draftTeamId
            : hasDraftTeam
                ? Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")
                : null;

        Assert.Equal(
            expected,
            TournamentParticipantsTab.ShouldPreserveRosterDraftAfterRefresh(
                draftTeamId,
                selectedTeamId));
    }

    [Fact]
    public void RefreshStartedBeforeLeavingAndReturningToTournamentIsRejected()
    {
        var tournamentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        Assert.False(TournamentParticipantsTab.IsRefreshCurrent(
            tournamentId,
            tournamentId,
            expectedRouteVersion: 4,
            currentRouteVersion: 6,
            expectedRegistrationGeneration: 12,
            currentRegistrationGeneration: 14));
    }

    [Fact]
    public void RefreshWithMatchingTournamentRouteAndGenerationIsCurrent()
    {
        var tournamentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        Assert.True(TournamentParticipantsTab.IsRefreshCurrent(
            tournamentId,
            tournamentId,
            expectedRouteVersion: 4,
            currentRouteVersion: 4,
            expectedRegistrationGeneration: 12,
            currentRegistrationGeneration: 12));
    }

    [Fact]
    public void CandidateDiscoveryBatchesFiftyOneMembersWithoutExceedingBackendLimit()
    {
        var userIds = Enumerable.Range(1, 51)
            .Select(index => new Guid(index, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0))
            .ToArray();

        var batches = TournamentParticipantsTab.BatchRosterEligibilityUserIds(userIds);

        Assert.Equal(2, batches.Count);
        Assert.Equal(50, batches[0].Count);
        Assert.Single(batches[1]);
        Assert.Equal(userIds, batches.SelectMany(batch => batch).ToArray());
        Assert.All(batches, batch => Assert.InRange(batch.Count, 1, 50));
    }

    [Fact]
    public void CaptainTransferReconcilesExistingRosterToTheCurrentCaptain()
    {
        var formerCaptain = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var retainedMember = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var currentCaptain = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var reconciled = TournamentParticipantsTab.ReconcileRosterForCurrentCaptain(
            [formerCaptain, retainedMember],
            currentCaptain,
            requiredTeamSize: 2,
            [formerCaptain]);

        Assert.Equal([retainedMember, currentCaptain], reconciled);
    }

    [Fact]
    public void CaptainTransferForSinglePlayerTeamKeepsOnlyTheCurrentCaptain()
    {
        var formerCaptain = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var currentCaptain = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        var reconciled = TournamentParticipantsTab.ReconcileRosterForCurrentCaptain(
            [formerCaptain],
            currentCaptain,
            requiredTeamSize: 1,
            [formerCaptain]);

        Assert.Equal([currentCaptain], reconciled);
    }
}
