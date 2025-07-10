using Mercurius.LAN.Web.Models;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface ILANClient
    {
        [Get("/lan/games")]
        Task<List<Game>> GetGamesAsync();

        [Get("/lan/games/{id}")]
        Task<GameExtended?> GetGameByIdAsync(int id);
    }
}
