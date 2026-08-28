using System.Text.Json;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class MockRegistrationParityTests
{
    private static readonly Guid TournamentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TeamId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid CaptainId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid MemberId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    [Fact]
    public void TeamSizeOneAcceptsCaptainOnlyAndAllowsAnEditWithoutDuplicateFailure()
    {
        using var fixture = CreateFixture(teamSize: 1);

        var eligibility = fixture.Store.CheckTeamTournamentRegistrationEligibility("user", TournamentId, TeamId);
        Assert.True(eligibility.Eligible);

        var registration = fixture.Store.SubmitTeamTournamentRoster(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId] });

        Assert.Equal(TournamentRegistrationStatus.Active, registration.Status);
        Assert.Single(registration.RosterMembers);
        Assert.Equal(RosterMemberConfirmationStatus.AutoConfirmed, registration.RosterMembers[0].ConfirmationStatus);

        var duplicateEligibility = fixture.Store.CheckTeamRosterEligibility(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId] });

        Assert.False(duplicateEligibility.Eligible);
        Assert.Contains("team_already_registered", duplicateEligibility.ReasonCodes);
        Assert.Contains("duplicate_participation", duplicateEligibility.Candidates.Single().ReasonCodes);

        var editedRegistration = fixture.Store.SubmitTeamTournamentRoster(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId] });

        Assert.Equal(TournamentRegistrationStatus.Active, editedRegistration.Status);
    }

    [Fact]
    public void TeamMemberCannotDeleteCaptainOwnedRegistration()
    {
        using var fixture = CreateFixture(teamSize: 2);

        var registration = fixture.Store.SubmitTeamTournamentRoster(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId, MemberId] });

        Assert.Equal(TournamentRegistrationStatus.PendingConfirmation, registration.Status);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Store.DeleteTeamTournamentRegistration("admin", TournamentId, TeamId));

        Assert.Equal("Team registration not found.", exception.Message);
    }

    [Fact]
    public void RosterEligibilityReportsDuplicateAndUnknownCandidates()
    {
        using var fixture = CreateFixture(teamSize: 2);

        var duplicate = fixture.Store.CheckTeamRosterEligibility(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId, CaptainId] });

        Assert.False(duplicate.Eligible);
        Assert.Contains("roster_user_ids_must_be_unique", duplicate.ReasonCodes);

        var unknownUserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        var unknown = fixture.Store.CheckTeamRosterEligibility(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId, unknownUserId] });

        var unknownCandidate = unknown.Candidates.Single(candidate => candidate.UserId == unknownUserId);
        Assert.Contains("user_not_found", unknownCandidate.ReasonCodes);
        Assert.Contains("not_team_member", unknownCandidate.ReasonCodes);

        var nullRoster = fixture.Store.CheckTeamRosterEligibility("user", TournamentId, TeamId, null!);
        Assert.Contains("exact_roster_size_required", nullRoster.ReasonCodes);
        Assert.Contains("captain_required", nullRoster.ReasonCodes);
    }

    [Fact]
    public void PendingRosterConfirmationIsRevalidatedAndCannotBeRepeated()
    {
        using var fixture = CreateFixture(teamSize: 2);

        var registration = fixture.Store.SubmitTeamTournamentRoster(
            "user",
            TournamentId,
            TeamId,
            new SubmitTeamRosterDTO { TeamId = TeamId, UserIds = [CaptainId, MemberId] });
        var pendingMemberId = registration.RosterMembers
            .Single(member => member.User.Id == MemberId)
            .Id;

        var confirmed = fixture.Store.ConfirmTournamentRosterMember("admin", TournamentId, pendingMemberId);

        Assert.Equal(RosterMemberConfirmationStatus.Confirmed,
            confirmed.RosterMembers.Single(member => member.Id == pendingMemberId).ConfirmationStatus);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Store.ConfirmTournamentRosterMember("admin", TournamentId, pendingMemberId));
        Assert.Equal("Pending roster confirmation not found.", exception.Message);
    }

    [Fact]
    public void ClosedTournamentReturnsScheduleReasonAndBlocksMutations()
    {
        using var fixture = CreateFixture(teamSize: 1, status: TournamentStatus.InProgress);

        var eligibility = fixture.Store.CheckTeamTournamentRegistrationEligibility("user", TournamentId, TeamId);

        Assert.False(eligibility.Eligible);
        Assert.Contains("tournament_not_scheduled", eligibility.ReasonCodes);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.Store.DeleteTeamTournamentRegistration("user", TournamentId, TeamId));

        Assert.Equal("tournament_not_scheduled", exception.Message);
    }

    private static Fixture CreateFixture(int teamSize, TournamentStatus status = TournamentStatus.Scheduled)
    {
        var root = Path.Combine(Path.GetTempPath(), "mercurius-lan-registration-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "backend.json"), BuildFixtureJson(teamSize, status));

        var environment = new TestHostEnvironment(root);
        var options = Microsoft.Extensions.Options.Options.Create(new MockBackendOptions { DataFilePath = "backend.json" });
        return new Fixture(root, new MockBackendStore(environment, options));
    }

    private static string BuildFixtureJson(int teamSize, TournamentStatus status)
    {
        var captain = new
        {
            id = CaptainId,
            username = "captain",
            firstname = "Casey",
            lastname = "Captain",
            email = "captain@example.test",
            emailVerified = true,
            discordId = "captain#0001",
            steamId = "steam-captain",
            riotId = (string?)null,
            displayName = "Casey Captain",
            isDeleted = false,
            createdAtUtc = "2026-01-01T00:00:00Z",
            updatedAtUtc = "2026-01-01T00:00:00Z"
        };
        var member = new
        {
            id = MemberId,
            username = "member",
            firstname = "Mina",
            lastname = "Member",
            email = "member@example.test",
            emailVerified = true,
            discordId = "member#0001",
            steamId = "steam-member",
            riotId = (string?)null,
            displayName = "Mina Member",
            isDeleted = false,
            createdAtUtc = "2026-01-01T00:00:00Z",
            updatedAtUtc = "2026-01-01T00:00:00Z"
        };

        var document = new
        {
            tournaments = new[]
            {
                new
                {
                    id = TournamentId,
                    name = "Parity Cup",
                    startTime = "2026-06-01T10:00:00Z",
                    endTime = "2026-06-01T16:00:00Z",
                    plannedStartTime = "2026-06-01T10:00:00Z",
                    estimatedEndTime = "2026-06-01T16:00:00Z",
                    status = status.ToString(),
                    bracketType = "SingleElimination",
                    format = "BestOf3",
                    finalsFormat = "BestOf3",
                    participationMode = "Team",
                    teamSize,
                    placements = Array.Empty<object>(),
                    matches = Array.Empty<object>(),
                    users = Array.Empty<object>(),
                    teams = Array.Empty<object>(),
                    registrations = Array.Empty<object>()
                }
            },
            teams = new[]
            {
                new
                {
                    id = TeamId,
                    name = "Parity Team",
                    captainUserId = CaptainId,
                    members = new[] { captain, member },
                    teamInvites = Array.Empty<object>()
                }
            },
            users = new[] { captain, member },
            profiles = new[]
            {
                new { persona = "user", profile = new { isComplete = true, user = captain, email = captain.email, emailVerified = true } },
                new { persona = "admin", profile = new { isComplete = true, user = member, email = member.email, emailVerified = true } }
            },
            sponsors = Array.Empty<object>()
        };

        return JsonSerializer.Serialize(document);
    }

    private sealed class Fixture(string root, MockBackendStore store) : IDisposable
    {
        public MockBackendStore Store { get; } = store;

        public void Dispose()
        {
            if(Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private sealed class TestHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = typeof(MockRegistrationParityTests).Assembly.GetName().Name!;
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
