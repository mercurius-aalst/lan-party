using MercuriusAalstLanParty.Client.Models;

namespace MercuriusAalstLanParty.Client.Services
{
    public interface IGameService
    {
        Task<GameExtended?> GetGameByIdAsync(int id);
        Task<List<Game>> GetGamesAsync();
        Task<Game> RegisterForGameAsync();
    }
}