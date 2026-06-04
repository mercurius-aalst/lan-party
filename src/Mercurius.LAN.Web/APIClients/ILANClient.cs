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
        [Get("/v1/lan/games")]
        Task<List<Game>> GetGamesAsync();

        [Get("/v1/lan/games/{id}")]
        Task<GameExtended?> GetGameByIdAsync(Guid id);

        [Get("/v1/lan/search")]
        Task<SearchResponseDTO> SearchAsync(
            [AliasAs("query")] string query,
            [AliasAs("cursor")] string? cursor = null,
            [AliasAs("pageSize")] int? pageSize = null,
            CancellationToken cancellationToken = default);

        [Post("/v1/lan/games")]
        Task<GameExtended> CreateGameAsync([Body] MultipartFormDataContent content);

        [Patch("/v1/lan/games/{id}")]
        Task<GameExtended> UpdateGameAsync(Guid id, [Body] MultipartFormDataContent formData);

        [Put("/v1/lan/games/{id}/sponsors")]
        Task<GameExtended> ReplaceGameSponsorsAsync(Guid id, [Body] ReplaceGameSponsorsDTO sponsors);

        [Delete("/v1/lan/games/{id}")]
        Task DeleteGameAsync(Guid id);

        [Post("/v1/lan/games/{id}/users")]
        Task<GameExtended> RegisterUserForGameAsync(Guid id, [Body] RegisterGameUserDTO registration);

        [Delete("/v1/lan/games/{id}/users/{userId}")]
        Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId);

        [Post("/v1/lan/games/{id}/teams")]
        Task<GameExtended> RegisterTeamForGameAsync(Guid id, [Body] RegisterGameTeamDTO registration);

        [Delete("/v1/lan/games/{id}/teams/{teamId}")]
        Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId);

        [Post("/v1/lan/games/{id}/start")]
        Task StartGameAsync(Guid id);

        [Post("/v1/lan/games/{id}/complete")]
        Task<IEnumerable<Placement>> CompleteGameAsync(Guid id);

        [Post("/v1/lan/games/{id}/cancel")]
        Task CancelGameAsync(Guid id);

        [Post("/v1/lan/games/{id}/reset")]
        Task ResetGameAsync(Guid id);

        [Get("/v1/lan/matches/{id}")]
        Task<Match> GetMatchByIdAsync(Guid id);

        [Put("/v1/lan/matches/{id}")]
        Task<Match> UpdateMatchAsync(Guid id, [Body] UpdateMatchDTO match);

        [Get("/v1/lan/teams")]
        Task<List<Team>> GetTeamsAsync();

        [Get("/v1/lan/teams/{id}")]
        Task<Team> GetTeamByIdAsync(Guid id);

        [Get("/v1/lan/public/teams/{teamName}")]
        Task<PublicTeamProfileDTO> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);

        [Post("/v1/lan/teams")]
        Task<Team> CreateTeamAsync([Body] CreateTeamDTO team);

        [Delete("/v1/lan/teams/{id}/users/{userId}")]
        Task<Team> RemoveTeamMemberAsync(Guid id, Guid userId);

        [Put("/v1/lan/teams/{id}")]
        Task<Team> UpdateTeamAsync(Guid id, [Body] UpdateTeamDTO team);

        [Delete("/v1/lan/teams/{id}")]
        Task DeleteTeamAsync(Guid id);

        [Post("/v1/lan/teams/{id}/users/invite/{userId}")]
        Task<TeamInvite> InviteTeamUserAsync(Guid id, Guid userId);

        [Put("/v1/lan/teams/{id}/users/invite/{userId}")]
        Task<TeamInvite> RespondToTeamInviteAsync(Guid id, Guid userId, [Body] RespondTeamInviteDTO response);

        [Get("/v1/lan/teams/users/{userId}/invites")]
        Task<IEnumerable<TeamInvite>> GetUserTeamInvitesAsync(Guid userId);

        [Get("/v1/lan/public/users/{username}")]
        Task<PublicUserProfileDTO> GetPublicUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

        [Get("/v1/lan/sponsors")]
        Task<IEnumerable<Sponsor>> GetSponsorsAsync();

        [Get("/v1/lan/sponsors/{id}")]
        Task<Sponsor> GetSponsorByIdAsync(int id);

        [Post("/v1/lan/sponsors")]
        Task<Sponsor> CreateSponsorAsync([Body] MultipartFormDataContent createSponsorFormData);

        [Patch("/v1/lan/sponsors/{id}")]
        Task<Sponsor> UpdateSponsorAsync(int id, [Body] MultipartFormDataContent updateSponsorFormData);

        [Delete("/v1/lan/sponsors/{id}")]
        Task DeleteSponsorAsync(int id);
    }
}
