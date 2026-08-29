using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class MockMatchLifecycleTests
{
    private static readonly Guid FeaturedTournamentId = Guid.Parse("11111111-1111-1111-1111-111111111112");
    private static readonly Guid FeaturedGrandFinalId = Guid.Parse("31111111-1111-1111-1111-111111111115");

    [Fact]
    public void MockAdminForfeitAndReverseKeepExplicitLifecycleState()
    {
        var repositoryRoot = FindRepositoryRoot();
        var store = new MockBackendStore(
            new TestHostEnvironment(repositoryRoot),
            Microsoft.Extensions.Options.Options.Create(new MockBackendOptions
            {
                DataFilePath = Path.Combine(repositoryRoot, "src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")
            }));

        var initial = store.GetMatchActionState("admin", FeaturedGrandFinalId);
        Assert.Equal(MatchLifecycleState.AwaitingEndedConfirmation, initial.Match.LifecycleState);
        Assert.False(initial.CanForfeit);
        Assert.True(initial.CanForceForfeit);
        Assert.False(initial.CanResolve);
        Assert.Equal("match_not_disputed", initial.ResolveBlockedReason);

        var forfeited = store.ForfeitMatch(
            "admin",
            FeaturedGrandFinalId,
            new ForfeitMatchDTO { Participant = MatchParticipantSide.Participant1 });

        Assert.Equal(MatchLifecycleState.Forfeited, forfeited.LifecycleState);
        Assert.Equal(MatchResultKind.Forfeit, forfeited.ResultKind);
        Assert.Equal(1, forfeited.ForfeitedParticipantNumber);

        var reversed = store.ReverseMatch("admin", FeaturedGrandFinalId);

        Assert.Equal(MatchLifecycleState.Reversed, reversed.LifecycleState);
        Assert.Null(reversed.ResultKind);
        Assert.Null(reversed.Participant1Score);
        Assert.Null(reversed.Participant2Score);
        Assert.False(store.GetMatchActionState("admin", FeaturedGrandFinalId).CanReverse);
    }

    [Fact]
    public void MockReverseClearsWinnerAndLoserFromBothDownstreamPaths()
    {
        var repositoryRoot = FindRepositoryRoot();
        var store = new MockBackendStore(
            new TestHostEnvironment(repositoryRoot),
            Microsoft.Extensions.Options.Options.Create(new MockBackendOptions
            {
                DataFilePath = Path.Combine(repositoryRoot, "src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")
            }));

        store.SetTournamentLifecycleState(FeaturedTournamentId, Mercurius.LAN.Web.Models.Tournaments.TournamentStatus.Scheduled);
        store.SetTournamentLifecycleState(FeaturedTournamentId, Mercurius.LAN.Web.Models.Tournaments.TournamentStatus.InProgress);
        var before = store.GetTournament(FeaturedTournamentId)!;
        var source = before.Matches.Single(match => match.Id == Guid.Parse("31111111-1111-1111-1111-111111111001"));
        var winnerNextId = source.WinnerNextMatchId ?? throw new InvalidOperationException("Fixture winner path is missing.");
        var loserNextId = source.LoserNextMatchId ?? throw new InvalidOperationException("Fixture loser path is missing.");
        var participant1Id = source.TeamParticipant1Id;
        var participant2Id = source.TeamParticipant2Id;

        store.ForfeitMatch(
            "admin",
            source.Id,
            new ForfeitMatchDTO { Participant = MatchParticipantSide.Participant1 });
        var actionState = store.GetMatchActionState("admin", source.Id);
        Assert.True(actionState.CanReverse);

        store.ReverseMatch("admin", source.Id);
        var after = store.GetTournament(FeaturedTournamentId)!;
        var winnerNext = after.Matches.Single(match => match.Id == winnerNextId);
        var loserNext = after.Matches.Single(match => match.Id == loserNextId);

        Assert.DoesNotContain(participant1Id, new[] { winnerNext.TeamParticipant1Id, winnerNext.TeamParticipant2Id });
        Assert.DoesNotContain(participant2Id, new[] { winnerNext.TeamParticipant1Id, winnerNext.TeamParticipant2Id });
        Assert.DoesNotContain(participant1Id, new[] { loserNext.TeamParticipant1Id, loserNext.TeamParticipant2Id });
        Assert.DoesNotContain(participant2Id, new[] { loserNext.TeamParticipant1Id, loserNext.TeamParticipant2Id });
    }

    [Fact]
    public void MockAnonymousMatchStateUsesPublicProjectionWithoutPrivateReports()
    {
        var repositoryRoot = FindRepositoryRoot();
        var store = new MockBackendStore(
            new TestHostEnvironment(repositoryRoot),
            Microsoft.Extensions.Options.Options.Create(new MockBackendOptions
            {
                DataFilePath = Path.Combine(repositoryRoot, "src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")
            }));

        var state = store.GetMatchActionState("anonymous", FeaturedGrandFinalId);

        Assert.Null(state.AuthorizedParticipant);
        Assert.Null(state.Participant1ReportedScore1);
        Assert.Null(state.Participant1ReportedScore2);
        Assert.Null(state.Participant2ReportedScore1);
        Assert.Null(state.Participant2ReportedScore2);

        var publicMatch = store.GetMatch(FeaturedGrandFinalId);
        Assert.Equal(state.Match.Participant1Score, publicMatch.Participant1Score);
        Assert.Equal(state.Match.Participant2Score, publicMatch.Participant2Score);
        Assert.Null(publicMatch.Participant1ReportedScore1);
        Assert.Null(publicMatch.Participant2ReportedScore1);
    }

    [Fact]
    public async Task MockAnonymousLoginAndUnauthenticatedRequestsStayAnonymousInTournamentService()
    {
        var repositoryRoot = FindRepositoryRoot();
        var store = new MockBackendStore(
            new TestHostEnvironment(repositoryRoot),
            Microsoft.Extensions.Options.Options.Create(new MockBackendOptions
            {
                DataFilePath = Path.Combine(repositoryRoot, "src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")
            }));

        var anonymousPrincipal = DependencyExtensions.BuildMockPrincipal("anonymous", store);
        Assert.False(anonymousPrincipal.Identity?.IsAuthenticated);
        Assert.Equal("anonymous", anonymousPrincipal.FindFirst("mock_persona")?.Value);

        var loginContext = new DefaultHttpContext { User = anonymousPrincipal };
        var loginService = new MockTournamentService(store, new HttpContextAccessor { HttpContext = loginContext });
        var loginState = await loginService.GetMatchActionStateAsync(FeaturedGrandFinalId);

        var unauthenticatedContext = new DefaultHttpContext();
        var unauthenticatedService = new MockTournamentService(
            store,
            new HttpContextAccessor { HttpContext = unauthenticatedContext });
        var unauthenticatedState = await unauthenticatedService.GetMatchActionStateAsync(FeaturedGrandFinalId);

        Assert.Null(loginState.AuthorizedParticipant);
        Assert.Null(loginState.Participant1ReportedScore1);
        Assert.Null(loginState.Participant2ReportedScore1);
        Assert.Null(unauthenticatedState.AuthorizedParticipant);
        Assert.Null(unauthenticatedState.Participant1ReportedScore1);
        Assert.Null(unauthenticatedState.Participant2ReportedScore1);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while(directory != null)
        {
            if(File.Exists(Path.Combine(directory.FullName, "src", "Mercurius.LAN.Web", "Mercurius.LAN.Web.csproj")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the LAN party repository root.");
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Mercurius.LAN.Web.ContractTests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
