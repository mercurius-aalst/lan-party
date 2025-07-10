using Mercurius.LAN.Web.Models;

namespace Mercurius.LAN.Web.Services
{
    public interface IGameService
    {
        Task<List<Game>> GetGamesAsync();
        Task<GameExtended?> GetGameByIdAsync(int id);
        Task<Game> RegisterForGameAsync();
    }
}