using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class MockBackendParityTests
{
    private static readonly Guid FeaturedTournamentId = Guid.Parse("11111111-1111-1111-1111-111111111112");

    [Fact]
    public void FeaturedFixture_ProjectsEverySeededTeamRegistration()
    {
        var store = CreateStore();

        var tournament = store.GetTournament(FeaturedTournamentId);

        Assert.NotNull(tournament);
        Assert.Equal(16, tournament!.Teams.Count());
        Assert.Equal(tournament.Teams.Count(), tournament.Registrations.Count(registration =>
            registration.Kind == TournamentRegistrationKind.Team &&
            registration.Status == TournamentRegistrationStatus.Active));
    }

    [Fact]
    public void Lifecycle_StartCompleteAndResetMatchesLiveStateShape()
    {
        var store = CreateStore();

        store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Canceled);
        store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Scheduled);
        store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.InProgress);

        var started = store.GetTournament(FeaturedTournamentId)!;
        Assert.Equal(TournamentStatus.InProgress, started.Status);
        Assert.NotEmpty(started.Matches);
        Assert.All(started.Matches, match => Assert.Equal(FeaturedTournamentId, match.TournamentId));
        Assert.Contains(started.Matches, match => !match.IsLowerBracketMatch);
        Assert.Contains(started.Matches, match => match.IsLowerBracketMatch);
        Assert.True(started.Matches.Max(match => match.RoundNumber) > 1);
        Assert.Contains(started.Matches, match => match.WinnerNextMatchId.HasValue);
        Assert.All(started.Matches, match =>
        {
            Assert.True(match.EstimatedStartTime.HasValue);
            Assert.True(match.EstimatedEndTime > match.EstimatedStartTime);
        });
        Assert.NotNull(started.EstimatedEndTime);

        var completionStore = CreateStore();
        var existingFinalMatch = completionStore.GetTournament(FeaturedTournamentId)!.Matches
            .OrderByDescending(match => match.RoundNumber)
            .ThenByDescending(match => match.MatchNumber)
            .First();
        completionStore.UpdateMatch(existingFinalMatch.Id, new UpdateMatchDTO { Participant1Score = 1, Participant2Score = 0 });
        completionStore.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Completed);
        var completed = completionStore.GetTournament(FeaturedTournamentId)!;
        Assert.Equal(TournamentStatus.Completed, completed.Status);
        Assert.NotEmpty(completed.Placements);

        completionStore.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Scheduled);
        var reset = completionStore.GetTournament(FeaturedTournamentId)!;
        Assert.Equal(TournamentStatus.Scheduled, reset.Status);
        Assert.Empty(reset.Matches);
        Assert.Empty(reset.Placements);
        Assert.Null(reset.EstimatedEndTime);
        Assert.Equal(DateTime.MinValue, reset.StartTime);
        Assert.Equal(DateTime.MinValue, reset.EndTime);
    }

    [Fact]
    public void Lifecycle_RejectsInvalidTransitionsAndInsufficientParticipants()
    {
        var store = CreateStore();
        Assert.Throws<InvalidOperationException>(() =>
            store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Completed));

        store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Canceled);
        store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Scheduled);
        Assert.Throws<InvalidOperationException>(() =>
            store.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Completed));

        var emptyTournament = store.CreateTournament(new CreateTournamentDTO
        {
            Name = "Needs Participants",
            BracketType = BracketType.SingleElimination,
            Format = TournamentFormat.BestOf1,
            FinalsFormat = TournamentFormat.BestOf1,
            ParticipationMode = ParticipationMode.Individual,
            PlannedStartTime = DateTime.UtcNow.AddDays(1),
            AverageGameDurationMinutes = 30,
            RoundBreakDurationMinutes = 10
        });

        Assert.Throws<InvalidOperationException>(() =>
            store.SetTournamentLifecycleState(emptyTournament.Id, TournamentStatus.InProgress));

        var completionStore = CreateStore();
        var finalMatch = completionStore.GetTournament(FeaturedTournamentId)!.Matches
            .OrderByDescending(match => match.RoundNumber)
            .ThenByDescending(match => match.MatchNumber)
            .First();
        completionStore.UpdateMatch(finalMatch.Id, new UpdateMatchDTO { Participant1Score = 1, Participant2Score = 0 });
        completionStore.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Completed);
        Assert.Throws<InvalidOperationException>(() =>
            completionStore.SetTournamentLifecycleState(FeaturedTournamentId, TournamentStatus.Canceled));
    }

    private static MockBackendStore CreateStore()
    {
        var repositoryRoot = FindRepositoryRoot();
        return new MockBackendStore(
            new TestHostEnvironment(repositoryRoot),
            Microsoft.Extensions.Options.Options.Create(new MockBackendOptions
            {
                DataFilePath = Path.Combine("src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")
            }));
    }

    private static string FindRepositoryRoot()
    {
        foreach(var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var directory = new DirectoryInfo(start);
            while(directory is not null)
            {
                if(File.Exists(Path.Combine(directory.FullName, "src", "Mercurius.LAN.Web", "MockData.Local", "backend.json")))
                    return directory.FullName;

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root for the mock fixture.");
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = nameof(MockBackendParityTests);
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(contentRootPath);
    }
}
