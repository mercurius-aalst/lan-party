using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Http;

namespace Mercurius.LAN.Web.Mock;

internal sealed class MockGameService : IGameService
{
    private readonly MockBackendStore _store;

    public MockGameService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<List<Game>> GetGamesAsync() => Task.FromResult(_store.GetGames());

    public Task<GameExtended?> GetGameByIdAsync(Guid id) => Task.FromResult(_store.GetGame(id));

    public Task<GameExtended> CreateGameAsync(CreateGameDTO newGame, string? tempFilePath, string? contentType, string? fileName) =>
        Task.FromResult(_store.CreateGame(newGame));

    public Task<Game> UpdateGameAsync(Guid id, UpdateGameDTO updatedGame, string? tempFilePath, string? contentType, string? fileName) =>
        Task.FromResult(_store.UpdateGame(id, updatedGame));

    public Task<GameExtended?> GetGameDetailAsync(Guid id) => Task.FromResult(_store.GetGame(id));

    public Task StartGameAsync(Guid id)
    {
        _store.StartGame(id);
        return Task.CompletedTask;
    }

    public Task CancelGameAsync(Guid id)
    {
        _store.CancelGame(id);
        return Task.CompletedTask;
    }

    public Task ResetGameAsync(Guid id)
    {
        _store.ResetGame(id);
        return Task.CompletedTask;
    }

    public Task DeleteGameAsync(Guid id)
    {
        _store.DeleteGame(id);
        return Task.CompletedTask;
    }

    public Task<GameExtended> RegisterUserForGameAsync(Guid id, Guid userId) => Task.FromResult(_store.RegisterUser(id, userId));

    public Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId) => Task.FromResult(_store.UnregisterUser(id, userId));

    public Task<GameExtended> RegisterTeamForGameAsync(Guid id, Guid teamId) => Task.FromResult(_store.RegisterTeam(id, teamId));

    public Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId) => Task.FromResult(_store.UnregisterTeam(id, teamId));

    public Task<Match> UpdateMatchScoresAsync(Guid matchId, UpdateMatchDTO updateMatchDTO) => Task.FromResult(_store.UpdateMatch(matchId, updateMatchDTO));

    public Task CompleteGameAsync(Guid id)
    {
        _store.CompleteGame(id);
        return Task.CompletedTask;
    }
}

internal sealed class MockTeamService : ITeamService
{
    private readonly MockBackendStore _store;

    public MockTeamService(MockBackendStore store)
    {
        _store = store;
    }

    public Task<List<Team>> GetTeamsAsync() => Task.FromResult(_store.GetTeams());

    public Task<Team> CreateTeamAsync(CreateTeamDTO team) => Task.FromResult(_store.CreateTeam(team));

    public Task<Team> UpdateTeamAsync(Guid id, UpdateTeamDTO team) => Task.FromResult(_store.UpdateTeam(id, team));

    public Task DeleteTeamAsync(Guid id)
    {
        _store.DeleteTeam(id);
        return Task.CompletedTask;
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

    private string GetCurrentPersona()
    {
        var persona = _httpContextAccessor.HttpContext?.User.FindFirst("mock_persona")?.Value;
        return string.IsNullOrWhiteSpace(persona) ? "user" : persona;
    }
}
