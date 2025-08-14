using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Models.Games;

namespace Mercurius.LAN.Web.Services
{
    public class GameService : IGameService
    {
        private readonly ILANClient _lANClient;

        public GameService(ILANClient lANClient)
        {
            _lANClient = lANClient;
        }

        public Task<List<Game>> GetGamesAsync()
        {
            return _lANClient.GetGamesAsync();

        }

        public async Task<GameExtended?> GetGameByIdAsync(int id)
        {
            return await _lANClient.GetGameByIdAsync(id);
        }

        public Task<Game> RegisterForGameAsync() => throw new NotImplementedException();

        public async Task<GameExtended> CreateGameAsync(CreateGameDTO newGame)
        {
            return await _lANClient.CreateGameAsync(newGame);
        }

        public async Task<Game> UpdateGameAsync(int id, UpdateGameDTO updatedGame)
        {
            return await _lANClient.UpdateGameAsync(id, updatedGame);
        }

        public async Task<GameExtended?> GetGameDetailAsync(int id)
        {
            return await _lANClient.GetGameByIdAsync(id);
        }

        public async Task StartGameAsync(int id)
        {
            await _lANClient.StartGameAsync(id);
        }

        public async Task CancelGameAsync(int id)
        {
            await _lANClient.CancelGameAsync(id);
        }

        public async Task ResetGameAsync(int id)
        {
            await _lANClient.ResetGameAsync(id);
        }

        public async Task DeleteGameAsync(int id)
        {
            await _lANClient.DeleteGameAsync(id);
        }
    }
}
