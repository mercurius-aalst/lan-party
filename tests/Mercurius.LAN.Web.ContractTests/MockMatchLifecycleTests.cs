using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class MockMatchLifecycleTests
{
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
