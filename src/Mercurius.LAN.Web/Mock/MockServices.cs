using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.DTOs.Search;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Http;
using ModelTournamentStatus = Mercurius.LAN.Web.Models.Tournaments.TournamentStatus;

namespace Mercurius.LAN.Web.Mock;

internal sealed class MockTournamentService : ITournamentService
{
    private readonly MockBackendStore _store;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockTournamentService(MockBackendStore store, IHttpContextAccessor httpContextAccessor)
    {
        _store = store;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<Tournament>> GetTournamentsAsync(
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetTournaments(page, pageSize));

    public Task<TournamentExtended?> GetTournamentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetTournament(id));

    public Task<TournamentExtended?> GetTournamentDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetTournament(id));

    public Task<TournamentExtended> CreateTournamentAsync(
        CreateTournamentDTO newTournament,
        string? tempFilePath,
        string? contentType,
        string? fileName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.CreateTournament(newTournament));

    public Task<TournamentExtended> UpdateTournamentAsync(
        Guid id,
        UpdateTournamentDTO updatedTournament,
        string? tempFilePath,
        string? contentType,
        string? fileName,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.UpdateTournament(id, updatedTournament));

    public Task SetTournamentLifecycleStateAsync(
        Guid id,
        ModelTournamentStatus state,
        CancellationToken cancellationToken = default)
    {
        _store.SetTournamentLifecycleState(id, state);
        return Task.CompletedTask;
    }

    public Task DeleteTournamentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.DeleteTournament(id);
        return Task.CompletedTask;
    }

    public Task<TournamentExtended> ReplaceTournamentSponsorsAsync(
        Guid id,
        ReplaceTournamentSponsorsDTO sponsors,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ReplaceTournamentSponsors(id, sponsors));

    public Task<Match> GetMatchByIdAsync(
        Guid matchId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetMatch(matchId));

    public Task<MatchActionStateDTO> GetMatchActionStateAsync(
        Guid matchId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetMatchActionState(GetCurrentPersona(), matchId));

    public Task<Match> ConfirmMatchEndedAsync(
        Guid matchId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ConfirmMatchEnded(GetCurrentPersona(), matchId));

    public Task<Match> SubmitMatchScoreAsync(
        Guid matchId,
        SubmitMatchScoreDTO request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.SubmitMatchScore(GetCurrentPersona(), matchId, request));

    public Task<Match> ForfeitMatchAsync(
        Guid matchId,
        ForfeitMatchDTO request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ForfeitMatch(GetCurrentPersona(), matchId, request));

    public Task<Match> ResolveMatchAsync(
        Guid matchId,
        ResolveMatchDTO request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ResolveMatch(GetCurrentPersona(), matchId, request));

    public Task<Match> ReverseMatchAsync(
        Guid matchId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ReverseMatch(GetCurrentPersona(), matchId));

    public Task<Match> UpdateMatchScoresAsync(
        Guid matchId,
        UpdateMatchDTO updateMatchDTO,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.UpdateMatch(matchId, updateMatchDTO));

    public Task<CurrentUserTournamentRegistrationStateDTO> GetCurrentUserTournamentRegistrationStateAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetCurrentUserTournamentRegistrationState(GetCurrentPersona(), tournamentId));

    public Task<EligibilityResponseDTO> CheckIndividualTournamentRegistrationEligibilityAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.CheckIndividualTournamentRegistrationEligibility(GetCurrentPersona(), tournamentId));

    public Task<EligibilityResponseDTO> CheckTeamTournamentRegistrationEligibilityAsync(
        Guid tournamentId,
        Guid teamId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.CheckTeamTournamentRegistrationEligibility(GetCurrentPersona(), tournamentId, teamId));

    public Task<RosterCandidateEligibilityResponseDTO> CheckTeamRosterEligibilityAsync(
        Guid tournamentId,
        Guid teamId,
        SubmitTeamRosterDTO roster,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.CheckTeamRosterEligibility(GetCurrentPersona(), tournamentId, teamId, roster));

    public Task<TournamentRegistrationDTO> RegisterCurrentUserForTournamentAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.RegisterCurrentUserForTournament(GetCurrentPersona(), tournamentId));

    public Task DeleteCurrentUserTournamentRegistrationAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default)
    {
        _store.DeleteCurrentUserTournamentRegistration(GetCurrentPersona(), tournamentId);
        return Task.CompletedTask;
    }

    public Task<TournamentRegistrationDTO> SubmitTeamTournamentRosterAsync(
        Guid tournamentId,
        Guid teamId,
        SubmitTeamRosterDTO roster,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.SubmitTeamTournamentRoster(GetCurrentPersona(), tournamentId, teamId, roster));

    public Task DeleteTeamTournamentRegistrationAsync(
        Guid tournamentId,
        Guid teamId,
        CancellationToken cancellationToken = default)
    {
        _store.DeleteTeamTournamentRegistration(GetCurrentPersona(), tournamentId, teamId);
        return Task.CompletedTask;
    }

    public Task<TournamentRegistrationDTO> ConfirmTournamentRosterMemberAsync(
        Guid tournamentId,
        Guid rosterMemberId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.ConfirmTournamentRosterMember(GetCurrentPersona(), tournamentId, rosterMemberId));

    public Task<List<AdminTournamentRegistrationDTO>> GetAdminTournamentRegistrationsAsync(
        Guid tournamentId,
        int? page = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetAdminTournamentRegistrations(tournamentId, page, pageSize));

    public Task RemoveTournamentUserRegistrationAsAdminAsync(
        Guid tournamentId,
        Guid userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _store.RemoveTournamentUserRegistrationAsAdmin(tournamentId, userId, reason);
        return Task.CompletedTask;
    }

    public Task RemoveTournamentTeamRegistrationAsAdminAsync(
        Guid tournamentId,
        Guid teamId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _store.RemoveTournamentTeamRegistrationAsAdmin(tournamentId, teamId, reason);
        return Task.CompletedTask;
    }

    private string GetCurrentPersona()
    {
        var persona = _httpContextAccessor.HttpContext?.User.FindFirst("mock_persona")?.Value;
        return string.IsNullOrWhiteSpace(persona) ? "user" : persona;
    }
}

internal sealed class MockTeamService : ITeamService
{
    private readonly MockBackendStore _store;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockTeamService(MockBackendStore store, IHttpContextAccessor httpContextAccessor)
    {
        _store = store;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<TeamPage> GetTeamsAsync(
        int page = 1,
        int pageSize = TeamPage.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if(page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");

        if(pageSize is < 1 or > TeamPage.MaximumPageSize)
            throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size must be between 1 and {TeamPage.MaximumPageSize}.");

        cancellationToken.ThrowIfCancellationRequested();

        var requestPageSize = pageSize < TeamPage.MaximumPageSize
            ? pageSize + 1
            : pageSize;
        var teams = _store.GetTeams(page, requestPageSize);
        var hasMore = teams.Count > pageSize;

        if(pageSize == TeamPage.MaximumPageSize && teams.Count == pageSize && page < int.MaxValue)
            hasMore = _store.GetTeams(page + 1, pageSize).Count > 0;

        return Task.FromResult(new TeamPage(teams.Take(pageSize).ToList(), page, pageSize, hasMore));
    }

    public Task<PublicTeamProfileDTO?> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetPublicTeamByName(teamName));

    public Task<CurrentUserTeamSummaryDTO> GetCurrentUserTeamSummaryAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.GetCurrentUserTeamSummary(GetCurrentPersona()));

    public Task<Team> CreateTeamAsync(CreateTeamDTO team)
    {
        var summary = _store.CreateCurrentUserTeam(GetCurrentPersona(), team);
        return Task.FromResult(new Team
        {
            Id = summary.Id,
            Name = summary.Name,
            CaptainUserId = summary.CaptainUserId,
            LogoUrl = summary.LogoUrl,
            Members = summary.Members
        });
    }

    public Task<TeamInvite> InviteUserAsync(Guid teamId, Guid userId) => Task.FromResult(_store.CreateTeamInvite(GetCurrentPersona(), teamId, userId));

    public Task<TeamInvite> CancelInviteAsync(Guid teamId, Guid inviteId) => Task.FromResult(_store.CancelTeamInvite(GetCurrentPersona(), teamId, inviteId));

    public Task<TeamInvite> RespondToInviteAsync(Guid inviteId, bool accept) => Task.FromResult(_store.RespondToTeamInvite(GetCurrentPersona(), inviteId, accept));

    public Task<TeamManagementSummaryDTO> LeaveTeamAsync(Guid teamId) => Task.FromResult(_store.LeaveTeam(GetCurrentPersona(), teamId));

    public Task<TeamManagementSummaryDTO> RemoveMemberAsync(Guid teamId, Guid userId) => Task.FromResult(_store.RemoveTeamMember(GetCurrentPersona(), teamId, userId));

    public Task<TeamManagementSummaryDTO> TransferCaptainAsync(Guid teamId, Guid newCaptainUserId) => Task.FromResult(_store.TransferCaptain(GetCurrentPersona(), teamId, newCaptainUserId));

    public Task<TeamLogoResponseDTO> UploadLogoAsync(Guid teamId, Stream logoStream, string contentType, string fileName) =>
        Task.FromResult(_store.UploadTeamLogo(GetCurrentPersona(), teamId, contentType, fileName));

    public Task<TeamLogoResponseDTO> RemoveLogoAsync(Guid teamId) => Task.FromResult(_store.RemoveTeamLogo(GetCurrentPersona(), teamId));

    public Task DeleteTeamAsync(Guid teamId)
    {
        _store.DeleteCurrentUserTeam(GetCurrentPersona(), teamId);
        return Task.CompletedTask;
    }

    private string GetCurrentPersona()
    {
        var persona = _httpContextAccessor.HttpContext?.User.FindFirst("mock_persona")?.Value;
        return string.IsNullOrWhiteSpace(persona) ? "user" : persona;
    }
}

internal sealed class MockGlobalSearchService : IGlobalSearchService
{
    private readonly MockBackendStore _store;

    public MockGlobalSearchService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<SearchResponseDTO> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = _store.SearchGlobal(query)
            .Select(result => result.Type == GlobalSearchResultType.User
                ? new GlobalSearchResultDTO
                {
                    Type = result.Type,
                    DisplayLabel = result.DisplayLabel,
                    SupportingText = result.SupportingText,
                    Username = result.Username
                }
                : result)
            .ToList();
        return Task.FromResult(new SearchResponseDTO
        {
            Results = results,
            NextCursor = null,
            HasMore = false
        });
    }
}

internal sealed class MockUserSearchService : IUserSearchService
{
    private readonly MockBackendStore _store;

    public MockUserSearchService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<GlobalSearchResultDTO>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<GlobalSearchResultDTO> results = _store.SearchGlobal(query)
            .Where(result => result.Type == GlobalSearchResultType.User && result.UserId.HasValue)
            .Take(6)
            .ToList();

        return Task.FromResult(results);
    }
}

internal sealed class MockPublicProfileService : IPublicProfileService
{
    private readonly MockBackendStore _store;

    public MockPublicProfileService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<PublicUserProfileDTO?> GetPublicUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_store.GetPublicUserByUsername(username));
    }
}

internal sealed class MockSponsorService : ISponsorService
{
    private readonly MockBackendStore _store;

    public MockSponsorService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<IEnumerable<Sponsor>> GetSponsorsAsync() => Task.FromResult<IEnumerable<Sponsor>>(_store.GetSponsors());

    public Task<Sponsor> GetSponsorByIdAsync(int id) => Task.FromResult(_store.GetSponsorById(id));

    public Task<Sponsor> CreateSponsorAsync(SponsorManagementDTO createSponsorDTO, string? tempFilePath, string? contentType, string? fileName) =>
        Task.FromResult(_store.CreateSponsor(createSponsorDTO));

    public Task<Sponsor> UpdateSponsorAsync(int id, SponsorManagementDTO updateSponsorDTO, string? tempFilePath, string? contentType, string? fileName) =>
        Task.FromResult(_store.UpdateSponsor(id, updateSponsorDTO));

    public Task DeleteSponsorAsync(int id)
    {
        _store.DeleteSponsor(id);
        return Task.CompletedTask;
    }
}

internal sealed class MockUserClient : IUserClient
{
    private readonly MockBackendStore _store;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockUserClient(MockBackendStore store, IHttpContextAccessor httpContextAccessor)
    {
        _store = store;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<CurrentUserProfileResponse> GetCurrentUserProfileAsync() =>
        Task.FromResult(_store.GetCurrentProfile(GetCurrentPersona()));

    public Task<UserProfileDTO> CompleteCurrentUserProfileAsync(CompleteUserProfileRequest request) =>
        Task.FromResult(_store.CompleteCurrentProfile(GetCurrentPersona(), request));

    public Task<UserProfileDTO> UpdateCurrentUserProfileAsync(UpdateUserProfileRequest request) =>
        Task.FromResult(_store.UpdateCurrentProfile(GetCurrentPersona(), request));

    public Task<UsernameAvailabilityResponse> CheckUsernameAvailabilityAsync(string username) =>
        Task.FromResult(_store.CheckUsernameAvailability(GetCurrentPersona(), username));

    public Task<UserActionResponse> ResendVerificationEmailAsync() =>
        Task.FromResult(new UserActionResponse("Mock verification email sent."));

    public Task<UserActionResponse> SendPasswordResetEmailAsync() =>
        Task.FromResult(new UserActionResponse("Mock password reset email sent."));

    public Task<UserActionResponse> DeleteCurrentUserAsync() =>
        Task.FromResult(_store.DeleteCurrentUser(GetCurrentPersona()));

    public Task<IEnumerable<UserDTO>> GetAllUsersAsync() => Task.FromResult<IEnumerable<UserDTO>>(_store.GetUsers());

    public Task<UserDTO> GetUserByIdAsync(Guid id) => Task.FromResult(_store.GetUserById(id));

    public Task DeleteUserAsync(string username)
    {
        _store.DeleteUser(username);
        return Task.CompletedTask;
    }

    public Task<UserDTO> CreateUserAsync(CreateUserProfileRequest request)
    {
        throw new NotSupportedException("Mock admin user creation is not implemented.");
    }

    public Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserProfileRequest request)
    {
        throw new NotSupportedException("Mock admin user updates are not implemented.");
    }

    public Task DeleteUserAsync(Guid id)
    {
        var user = _store.GetUserById(id);
        if(!string.IsNullOrWhiteSpace(user.Username))
            _store.DeleteUser(user.Username);

        return Task.CompletedTask;
    }

    public Task DeleteUserAccountAsync(string username)
    {
        _store.DeleteUser(username);
        return Task.CompletedTask;
    }

    private string GetCurrentPersona()
    {
        var persona = _httpContextAccessor.HttpContext?.User.FindFirst("mock_persona")?.Value;
        return string.IsNullOrWhiteSpace(persona) ? "user" : persona;
    }
}
