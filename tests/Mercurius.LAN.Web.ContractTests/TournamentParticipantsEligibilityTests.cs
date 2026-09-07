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

    [Fact]
    public void FreshRosterAutofillKeepsCaptainAndSelectsOnlyEligibleMembersToExactSize()
    {
        var captainId = Guid.NewGuid();
        var eligibleMemberId = Guid.NewGuid();
        var secondEligibleMemberId = Guid.NewGuid();
        var unavailableMemberId = Guid.NewGuid();
        var members = new[] { captainId, unavailableMemberId, eligibleMemberId, secondEligibleMemberId };
        var candidates = new Dictionary<Guid, RosterCandidateEligibilityDTO>
        {
            [unavailableMemberId] = Candidate(unavailableMemberId, false, "duplicate_participation"),
            [eligibleMemberId] = Candidate(eligibleMemberId, true),
            [secondEligibleMemberId] = Candidate(secondEligibleMemberId, true)
        };

        var selection = TournamentParticipantsTab.NormalizeRosterSelection(
            [captainId],
            members,
            captainId,
            candidates,
            new HashSet<Guid>(),
            autofillToSize: 3);

        Assert.Equal([captainId, eligibleMemberId, secondEligibleMemberId], selection);
        Assert.DoesNotContain(unavailableMemberId, selection);
    }

    [Fact]
    public void RestoredRosterDropsUnavailableAndStaleMembersButKeepsAllowedSavedConflict()
    {
        var captainId = Guid.NewGuid();
        var eligibleMemberId = Guid.NewGuid();
        var savedConflictMemberId = Guid.NewGuid();
        var unavailableMemberId = Guid.NewGuid();
        var staleMemberId = Guid.NewGuid();
        var members = new[] { captainId, eligibleMemberId, savedConflictMemberId, unavailableMemberId };
        var candidates = new Dictionary<Guid, RosterCandidateEligibilityDTO>
        {
            [eligibleMemberId] = Candidate(eligibleMemberId, true),
            [savedConflictMemberId] = Candidate(savedConflictMemberId, false, "duplicate_participation"),
            [unavailableMemberId] = Candidate(unavailableMemberId, false, "user_not_team_member")
        };

        var selection = TournamentParticipantsTab.NormalizeRosterSelection(
            [captainId, eligibleMemberId, savedConflictMemberId, unavailableMemberId, staleMemberId],
            members,
            captainId,
            candidates,
            new HashSet<Guid> { savedConflictMemberId });

        Assert.Equal([captainId, eligibleMemberId, savedConflictMemberId], selection);
    }

    [Fact]
    public void ExistingSavedRosterMemberAllowsOnlyDuplicateParticipationConflict()
    {
        var captainId = Guid.NewGuid();
        var savedConflictMemberId = Guid.NewGuid();
        var broaderConflictMemberId = Guid.NewGuid();
        var members = new HashSet<Guid> { captainId, savedConflictMemberId, broaderConflictMemberId };
        var candidates = new Dictionary<Guid, RosterCandidateEligibilityDTO>
        {
            [savedConflictMemberId] = Candidate(savedConflictMemberId, false, "duplicate_participation"),
            [broaderConflictMemberId] = Candidate(broaderConflictMemberId, false, "team_already_registered")
        };
        var existingRoster = new HashSet<Guid> { savedConflictMemberId, broaderConflictMemberId };

        Assert.True(TournamentParticipantsTab.IsRosterMemberSelectable(
            savedConflictMemberId,
            captainId,
            members,
            candidates,
            existingRoster));
        Assert.False(TournamentParticipantsTab.IsRosterMemberSelectable(
            broaderConflictMemberId,
            captainId,
            members,
            candidates,
            existingRoster));
    }

    [Fact]
    public void EligibilityMergeRemovesARealtimeInvalidatedMemberFromNormalizedSelection()
    {
        var captainId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var candidates = new Dictionary<Guid, RosterCandidateEligibilityDTO>
        {
            [memberId] = Candidate(memberId, true)
        };

        TournamentParticipantsTab.MergeRosterCandidateEligibility(
            candidates,
            [Candidate(memberId, false, "duplicate_participation")]);

        var selection = TournamentParticipantsTab.NormalizeRosterSelection(
            [captainId, memberId],
            [captainId, memberId],
            captainId,
            candidates,
            new HashSet<Guid>());

        Assert.Equal([captainId], selection);
        Assert.False(candidates[memberId].Eligible);
    }

    [Fact]
    public void UnavailableMemberIsRejectedByTogglePredicateAndReviewRequestFiltering()
    {
        var captainId = Guid.NewGuid();
        var eligibleMemberId = Guid.NewGuid();
        var unavailableMemberId = Guid.NewGuid();
        var members = new HashSet<Guid> { captainId, eligibleMemberId, unavailableMemberId };
        var candidates = new Dictionary<Guid, RosterCandidateEligibilityDTO>
        {
            [eligibleMemberId] = Candidate(eligibleMemberId, true),
            [unavailableMemberId] = Candidate(unavailableMemberId, false, "user_not_team_member")
        };

        Assert.False(TournamentParticipantsTab.IsRosterMemberSelectable(
            unavailableMemberId,
            captainId,
            members,
            candidates,
            new HashSet<Guid>()));

        var filteredForReviewAndRequest = TournamentParticipantsTab.NormalizeRosterSelection(
            [captainId, eligibleMemberId, unavailableMemberId],
            members.ToArray(),
            captainId,
            candidates,
            new HashSet<Guid>());

        Assert.Equal([captainId, eligibleMemberId], filteredForReviewAndRequest);
    }

    private static RosterCandidateEligibilityDTO Candidate(
        Guid userId,
        bool eligible,
        params string[] reasons) => new()
        {
            UserId = userId,
            Eligible = eligible,
            ReasonCodes = reasons
        };
}
