using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.Services
{
    public interface IGameService
    {
        Task<List<Game>> GetGamesAsync();
        Task<GameExtended?> GetGameByIdAsync(int id);
        Task<Game> RegisterForGameAsync();
    }
}