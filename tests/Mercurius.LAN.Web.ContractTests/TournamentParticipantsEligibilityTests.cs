using Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;
using Mercurius.LAN.Web.DTOs.Registrations;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class TournamentParticipantsEligibilityTests
{
    [Fact]
    public void ExistingTeamConflictsDoNotHideCaptainRosterActions()
    {
        var eligibility = new EligibilityResponseDTO
        {
            Eligible = false,
            ReasonCodes = ["team_already_registered", "captain_duplicate_participation"]
        };

        Assert.True(TournamentParticipantsTab.IsExistingRegistrationEligibilityUsable(eligibility));
    }

    [Fact]
    public void UnrelatedTeamConflictsStillBlockCaptainRosterActions()
    {
        var eligibility = new EligibilityResponseDTO
        {
            Eligible = false,
            ReasonCodes = ["team_already_registered", "tournament_not_scheduled"]
        };

        Assert.False(TournamentParticipantsTab.IsExistingRegistrationEligibilityUsable(eligibility));
    }

    [Fact]
    public void ExistingRosterConflictsAreAllowedOnlyForExistingMembers()
    {
        var existingUserId = Guid.NewGuid();
        var newUserId = Guid.NewGuid();
        var eligibility = new RosterCandidateEligibilityResponseDTO
        {
            Eligible = false,
            ReasonCodes = ["duplicate_participation"],
            Candidates =
            [
                new()
                {
                    UserId = existingUserId,
                    Eligible = false,
                    ReasonCodes = ["duplicate_participation"]
                },
                new()
                {
                    UserId = newUserId,
                    Eligible = false,
                    ReasonCodes = ["user_not_team_member"]
                }
            ]
        };

        Assert.False(TournamentParticipantsTab.IsExistingRosterEligibilityUsable(
            eligibility,
            new HashSet<Guid> { existingUserId }));
        Assert.True(TournamentParticipantsTab.IsExistingRosterEligibilityUsable(
            new RosterCandidateEligibilityResponseDTO
            {
                Eligible = false,
                ReasonCodes = ["duplicate_participation"],
                Candidates = [eligibility.Candidates[0]]
            },
            new HashSet<Guid> { existingUserId }));
    }

    [Fact]
    public void EmptyConflictReasonsDoNotOverrideEligibilityFailure()
    {
        Assert.False(TournamentParticipantsTab.IsExistingRegistrationEligibilityUsable(
            new EligibilityResponseDTO { Eligible = false }));
        Assert.False(TournamentParticipantsTab.IsExistingRosterEligibilityUsable(
            new RosterCandidateEligibilityResponseDTO { Eligible = false },
            new HashSet<Guid>()));
    }
}
