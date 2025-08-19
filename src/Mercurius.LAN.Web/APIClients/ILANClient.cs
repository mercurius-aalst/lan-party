using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.DTOs.Participants.Players;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Refit;
using System.Net.Http.Headers;

namespace Mercurius.LAN.Web.APIClients
{
    public interface ILANClient
    {
        [Get("/lan/games")]
        Task<List<Game>> GetGamesAsync();

        [Get("/lan/games/{id}")]
        Task<GameExtended?> GetGameByIdAsync(int id);

        [Post("/lan/games")]
        Task<GameExtended> CreateGameAsync([Body] MultipartFormDataContent content);


        [Patch("/lan/games/{id}")]
        Task<Game> UpdateGameAsync(int id, [Body] MultipartFormDataContent formData);

        [Delete("/lan/games/{id}")]
        Task DeleteGameAsync(int id);

        [Post("/lan/games/{id}/participants/{participantId}")]
        Task<GameExtended> RegisterForGameAsync(int id, int participantId);

        [Delete("/lan/games/{id}/participants/{participantId}")]
        Task<GameExtended> UnregisterFromGameAsync(int id, int participantId);

        [Post("/lan/games/{id}/start")]
        Task StartGameAsync(int id);

        [Post("/lan/games/{id}/complete")]
        Task<IEnumerable<Placement>> CompleteGameAsync(int id);

        [Post("/lan/games/{id}/cancel")]
        Task CancelGameAsync(int id);

        [Post("/lan/games/{id}/reset")]
        Task ResetGameAsync(int id);



        [Get("/lan/matches/{id}")]
        Task<Match> GetMatchByIdAsync(int id);

        [Put("/lan/matches/{id}")]
        Task<Match> UpdateMatchAsync(int id, [Body] UpdateMatchDTO match);


        [Post("/lan/players")]
        Task<Player> CreatePlayerAsync([Body] CreatePlayerDTO player);

        [Patch("/lan/players/{id}")]
        Task<Player> UpdatePlayerAsync(int id, [Body] UpdatePlayerDTO player);

        [Delete("/lan/players/{id}")]
        Task DeletePlayerAsync(int id);

        [Get("/lan/players/{id}")]
        Task<Player> GetPlayerByIdAsync(int id);

        [Get("/lan/players")]
        Task<List<Player>> GetPlayersAsync();


        [Get("/lan/teams")]
        Task<List<Team>> GetTeamsAsync();
        [Get("/lan/teams/{id}")]
        Task<Team> GetTeamByIdAsync(int id);
        [Post("/lan/teams")]
        Task<Team> CreateTeamAsync([Body] CreateTeamDTO team);
        [Put("/lan/teams/{id}")]
        Task<Team> UpdateTeamAsync(int id, [Body] UpdateTeamDTO team);
        [Delete("/lan/teams/{id}")]
        Task DeleteTeamAsync(int id);


    }
}
