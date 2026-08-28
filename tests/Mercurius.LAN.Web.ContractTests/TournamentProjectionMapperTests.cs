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

    private static PublicUserDTO CreateUser(Guid id, string username) => new()
    {
        Id = id,
        Username = username,
        DisplayName = username
    };
}
