using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.DTOs.Search;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface ILANClient
    {
        [Get("/lan/games")]
        Task<List<Game>> GetGamesAsync();

        [Get("/lan/games/{id}")]
        Task<GameExtended?> GetGameByIdAsync(Guid id);

        // TODO(backend): implement GET /lan/search?query={query} endpoint that returns normalized user/team/game search results.
        [Get("/lan/search")]
        Task<List<GlobalSearchResultDTO>> SearchAsync([AliasAs("query")] string query, CancellationToken cancellationToken = default);

        [Post("/lan/games")]
        Task<GameExtended> CreateGameAsync([Body] MultipartFormDataContent content);

        [Patch("/lan/games/{id}")]
        Task<Game> UpdateGameAsync(Guid id, [Body] MultipartFormDataContent formData);

        [Put("/lan/games/{id}/sponsors")]
        Task<GameExtended> ReplaceGameSponsorsAsync(Guid id, [Body] ReplaceGameSponsorsDTO sponsors);

        [Delete("/lan/games/{id}")]
        Task DeleteGameAsync(Guid id);

        [Post("/lan/games/{id}/users")]
        Task<GameExtended> RegisterUserForGameAsync(Guid id, [Body] RegisterGameUserDTO registration);

        [Delete("/lan/games/{id}/users/{userId}")]
        Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId);

        [Post("/lan/games/{id}/teams")]
        Task<GameExtended> RegisterTeamForGameAsync(Guid id, [Body] RegisterGameTeamDTO registration);

        [Delete("/lan/games/{id}/teams/{teamId}")]
        Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId);

        [Post("/lan/games/{id}/start")]
        Task StartGameAsync(Guid id);

        [Post("/lan/games/{id}/complete")]
        Task<IEnumerable<Placement>> CompleteGameAsync(Guid id);

        [Post("/lan/games/{id}/cancel")]
        Task CancelGameAsync(Guid id);

        [Post("/lan/games/{id}/reset")]
        Task ResetGameAsync(Guid id);

        [Get("/lan/matches/{id}")]
        Task<Match> GetMatchByIdAsync(Guid id);

        [Put("/lan/matches/{id}")]
        Task<Match> UpdateMatchAsync(Guid id, [Body] UpdateMatchDTO match);

        [Get("/lan/teams")]
        Task<List<Team>> GetTeamsAsync();

        [Get("/lan/teams/{id}")]
        Task<Team> GetTeamByIdAsync(Guid id);

        // TODO(backend): implement GET /lan/public/teams/{teamName} endpoint with privacy-safe public team profile response.
        [Get("/lan/public/teams/{teamName}")]
        Task<PublicTeamProfileDTO> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);

        [Post("/lan/teams")]
        Task<Team> CreateTeamAsync([Body] CreateTeamDTO team);

        [Put("/lan/teams/{id}")]
        Task<Team> UpdateTeamAsync(Guid id, [Body] UpdateTeamDTO team);

        [Delete("/lan/teams/{id}")]
        Task DeleteTeamAsync(Guid id);

        // TODO(backend): implement GET /lan/public/users/{username} endpoint with privacy-safe public user profile response.
        [Get("/lan/public/users/{username}")]
        Task<PublicUserProfileDTO> GetPublicUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

        [Get("/lan/sponsors")]
        Task<IEnumerable<Sponsor>> GetSponsorsAsync();

        [Get("/lan/sponsors/{id}")]
        Task<Sponsor> GetSponsorByIdAsync(int id);

        [Post("/lan/sponsors")]
        Task<Sponsor> CreateSponsorAsync([Body] MultipartFormDataContent createSponsorFormData);

        [Patch("/lan/sponsors/{id}")]
        Task<Sponsor> UpdateSponsorAsync(int id, [Body] MultipartFormDataContent updateSponsorFormData);

        [Delete("/lan/sponsors/{id}")]
        Task DeleteSponsorAsync(int id);
    }
}
