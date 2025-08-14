using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.Services
{
    public interface IGameService
    {
        Task<List<Game>> GetGamesAsync();
        Task<GameExtended?> GetGameByIdAsync(int id);
        Task<GameExtended> CreateGameAsync(CreateGameDTO newGame);
        Task<Game> UpdateGameAsync(int id, UpdateGameDTO updatedGame);
        Task<GameExtended?> GetGameDetailAsync(int id);
        Task StartGameAsync(int id);
        Task CancelGameAsync(int id);
        Task ResetGameAsync(int id);
        Task DeleteGameAsync(int id);
        Task<GameExtended> RegisterForGameAsync(int id, int participantId);
        Task<GameExtended> UnregisterFromGameAsync(int id, int participantId);
    }
}