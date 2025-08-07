using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface ILANClient
    {
        [Get("/lan/games")]
        Task<List<Game>> GetGamesAsync();

        [Get("/lan/games/{id}")]
        Task<GameExtended?> GetGameByIdAsync(int id);

        [Get("/lan/matches/{id}")]
        Task<Match> GetMatchByIdAsync(int id);
    }
}
