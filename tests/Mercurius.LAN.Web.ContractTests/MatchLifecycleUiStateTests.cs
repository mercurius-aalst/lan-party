using Mercurius.LAN.Web.Components.Pages.Tournaments;
using Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.DetailView;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Tournaments;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class MatchLifecycleUiStateTests
{
    [Fact]
    public void InitialScores_PreferTheAuthorizedParticipantFullReport()
    {
        var state = CreateActionState(MatchParticipantSide.Participant2);
        state.Participant1ReportedScore1 = 2;
        state.Participant1ReportedScore2 = 0;
        state.Participant2ReportedScore1 = 1;
        state.Participant2ReportedScore2 = 0;

        var scores = TournamentMatchDetailsDialog.GetInitialScores(state);

        Assert.Equal(1, scores.Participant1Score);
        Assert.Equal(0, scores.Participant2Score);
    }

    [Fact]
    public void InitialScores_DoNotCombineMismatchedOrOneSidedReports()
    {
        var mismatchedAdminState = CreateActionState();
        mismatchedAdminState.Participant1ReportedScore1 = 2;
        mismatchedAdminState.Participant1ReportedScore2 = 0;
        mismatchedAdminState.Participant2ReportedScore1 = 1;
        mismatchedAdminState.Participant2ReportedScore2 = 0;

        var mismatchedScores = TournamentMatchDetailsDialog.GetInitialScores(mismatchedAdminState);

        Assert.Null(mismatchedScores.Participant1Score);
        Assert.Null(mismatchedScores.Participant2Score);

        var oneSidedState = CreateActionState(MatchParticipantSide.Participant1);
        oneSidedState.Participant1ReportedScore1 = 2;

        var oneSidedScores = TournamentMatchDetailsDialog.GetInitialScores(oneSidedState);

        Assert.Null(oneSidedScores.Participant1Score);
        Assert.Null(oneSidedScores.Participant2Score);
    }

    [Fact]
    public void AdminReports_AreRenderedOnlyWhenProtectedReportsArePresent()
    {
        var adminState = CreateActionState();
        adminState.Participant1ReportedScore1 = 2;
        adminState.Participant1ReportedScore2 = 0;
        adminState.Participant2ReportedScore1 = 1;
        adminState.Participant2ReportedScore2 = 0;

        Assert.True(TournamentMatchDetailsDialog.ShouldRenderAdminReports(adminState));

        adminState.AuthorizedParticipant = MatchParticipantSide.Participant1;

        Assert.True(TournamentMatchDetailsDialog.ShouldRenderAdminReports(adminState));

        adminState.Participant1ReportedScore1 = null;
        adminState.Participant1ReportedScore2 = null;
        adminState.Participant2ReportedScore1 = null;
        adminState.Participant2ReportedScore2 = null;

        Assert.False(TournamentMatchDetailsDialog.ShouldRenderAdminReports(adminState));
    }

    [Fact]
    public void ReconcileSelectedMatch_UsesTheRefreshedSameIdInstance()
    {
        var matchId = Guid.NewGuid();
        var selectedMatch = new Match
        {
            Id = matchId,
            LifecycleState = MatchLifecycleState.Disputed
        };
        var refreshedMatch = new Match
        {
            Id = matchId,
            LifecycleState = MatchLifecycleState.Completed,
            ResultVersion = 3
        };
        var tournament = new TournamentExtended { Matches = [refreshedMatch] };

        var reconciled = TournamentDetail.ReconcileSelectedMatch(selectedMatch, tournament);

        Assert.Same(refreshedMatch, reconciled);
        Assert.Equal(MatchLifecycleState.Completed, reconciled!.LifecycleState);
        Assert.Null(TournamentDetail.ReconcileSelectedMatch(selectedMatch, new TournamentExtended()));
    }

    [Fact]
    public void ChildRefresh_IsPreservedWhenParentReloadFails()
    {
        var matchId = Guid.NewGuid();
        var staleSelectedMatch = new Match
        {
            Id = matchId,
            LifecycleState = MatchLifecycleState.Disputed,
            ResultVersion = 2
        };
        var refreshedMatch = new Match
        {
            Id = matchId,
            LifecycleState = MatchLifecycleState.Completed,
            ResultVersion = 3
        };

        var tournament = new TournamentExtended { Matches = [staleSelectedMatch] };

        // The parent applies this projection before its tournament reload. A failed
        // reload must leave both the schedule/bracket row and dialog bound to the child
        // refresh, not the old same-ID object.
        var preserved = TournamentDetail.ApplyRefreshedMatchProjection(
            tournament,
            staleSelectedMatch,
            refreshedMatch);

        Assert.Same(refreshedMatch, preserved);
        Assert.Same(refreshedMatch, tournament.Matches.Single());
        Assert.Equal(MatchLifecycleState.Completed, preserved!.LifecycleState);
        Assert.Equal(3, preserved.ResultVersion);
    }

    private static MatchActionStateDTO CreateActionState(MatchParticipantSide? authorizedParticipant = null) => new()
    {
        Match = new Match { Id = Guid.NewGuid() },
        AuthorizedParticipant = authorizedParticipant
    };
}
