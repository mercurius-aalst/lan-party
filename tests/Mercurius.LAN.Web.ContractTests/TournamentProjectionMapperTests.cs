using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Tournaments;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class TournamentProjectionMapperTests
{
    [Fact]
    public void PopulateParticipantProjection_UsesRegistrationRosterMembers()
    {
        var decoyTeamMember = CreateUser(Guid.Parse("11111111-1111-1111-1111-111111111111"), "decoy");
        var captain = CreateUser(Guid.Parse("22222222-2222-2222-2222-222222222222"), "captain");
        var teammate = CreateUser(Guid.Parse("33333333-3333-3333-3333-333333333333"), "teammate");
        var team = new PublicTournamentTeamDTO
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Name = "Current Roster",
            CaptainUserId = captain.Id,
            Members = [decoyTeamMember]
        };
        var tournament = new TournamentExtended
        {
            Registrations =
            [
                new PublicTournamentRegistrationDTO
                {
                    Kind = TournamentRegistrationKind.Team,
                    Status = TournamentRegistrationStatus.Active,
                    Team = team,
                    RosterMembers =
                    [
                        new PublicTournamentRosterMemberDTO { User = captain, IsCaptain = true },
                        new PublicTournamentRosterMemberDTO { User = teammate, IsCaptain = false }
                    ]
                }
            ]
        };

        TournamentProjectionMapper.PopulateParticipantProjection(tournament);

        var projectedTeam = Assert.Single(tournament.Teams);
        Assert.Equal(team.Id, projectedTeam.Id);
        Assert.Equal(new[] { captain.Id, teammate.Id }, projectedTeam.Members.Select(member => member.Id));
        Assert.DoesNotContain(projectedTeam.Members, member => member.Id == decoyTeamMember.Id);
    }

    [Fact]
    public void PopulateParticipantProjection_ClearsParticipantsWhenRegistrationsAreEmpty()
    {
        var tournament = new TournamentExtended
        {
            Users = [CreateUser(Guid.NewGuid(), "stale-user")],
            Teams =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Stale Team",
                    CaptainUserId = Guid.NewGuid()
                }
            ],
            Registrations = []
        };

        TournamentProjectionMapper.PopulateParticipantProjection(tournament);

        Assert.Empty(tournament.Users);
        Assert.Empty(tournament.Teams);
    }

    [Fact]
    public void PopulateParticipantProjection_DeduplicatesRosterMembersAndTeams()
    {
        var captain = CreateUser(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "captain");
        var teammate = CreateUser(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "teammate");
        var team = new PublicTournamentTeamDTO
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            Name = "Duplicate Proof",
            CaptainUserId = captain.Id
        };
        var tournament = new TournamentExtended
        {
            Registrations =
            [
                new PublicTournamentRegistrationDTO
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    Kind = TournamentRegistrationKind.Team,
                    Status = TournamentRegistrationStatus.Active,
                    Team = team,
                    RosterMembers =
                    [
                        new() { User = captain, IsCaptain = true },
                        new() { User = teammate },
                        new() { User = teammate }
                    ]
                },
                new PublicTournamentRegistrationDTO
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Kind = TournamentRegistrationKind.Team,
                    Status = TournamentRegistrationStatus.Active,
                    Team = team,
                    RosterMembers = [new() { User = captain, IsCaptain = true }]
                }
            ]
        };

        TournamentProjectionMapper.PopulateParticipantProjection(tournament);

        var projectedTeam = Assert.Single(tournament.Teams);
        Assert.Equal(new[] { captain.Id, teammate.Id }, projectedTeam.Members.Select(member => member.Id));
    }

    [Fact]
    public void ApplyRegistration_ReplacesExistingTeamProjection()
    {
        var captain = CreateUser(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "captain");
        var oldTeammate = CreateUser(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "old");
        var newTeammate = CreateUser(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "new");
        var teamId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        var tournament = new TournamentExtended
        {
            Registrations =
            [
                new PublicTournamentRegistrationDTO
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    Kind = TournamentRegistrationKind.Team,
                    Status = TournamentRegistrationStatus.Active,
                    Team = new PublicTournamentTeamDTO
                    {
                        Id = teamId,
                        Name = "Replaceable",
                        CaptainUserId = captain.Id
                    },
                    RosterMembers =
                    [
                        new() { User = captain, IsCaptain = true },
                        new() { User = oldTeammate }
                    ]
                }
            ]
        };
        var replacement = new TournamentRegistrationDTO
        {
            Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
            TournamentId = tournament.Id,
            Kind = TournamentRegistrationKind.Team,
            Status = TournamentRegistrationStatus.Active,
            Team = new()
            {
                Id = teamId,
                Name = "Replaceable",
                CaptainUserId = captain.Id
            },
            RosterMembers =
            [
                new() { User = captain, IsCaptain = true },
                new() { User = newTeammate }
            ]
        };

        TournamentProjectionMapper.ApplyRegistration(tournament, replacement);

        var projectedTeam = Assert.Single(tournament.Teams);
        Assert.Equal(new[] { captain.Id, newTeammate.Id }, projectedTeam.Members.Select(member => member.Id));
        Assert.Equal(replacement.Id, Assert.Single(tournament.Registrations).Id);
    }

    private static PublicUserDTO CreateUser(Guid id, string username) => new()
    {
        Id = id,
        Username = username,
        DisplayName = username
    };
}
