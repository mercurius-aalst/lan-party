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

        public Task<GameExtended?> GetGameByIdAsync(int id)
        {
            return _lANClient.GetGameByIdAsync(id);
        }

        public Task<GameExtended> RegisterForGameAsync(int id, int participantId)
        {
            return _lANClient.RegisterForGameAsync(id, participantId);
        }

        public Task<GameExtended> UnregisterFromGameAsync(int id, int participantId)
        {
            return _lANClient.UnregisterFromGameAsync(id, participantId);
        }
        public Task<GameExtended> CreateGameAsync(CreateGameDTO newGame)
        {
            return _lANClient.CreateGameAsync(newGame);
        }

        public async Task<Game> UpdateGameAsync(int id, UpdateGameDTO updatedGame)
        {
            return await _lANClient.UpdateGameAsync(id, updatedGame);
        }

        public async Task<GameExtended?> GetGameDetailAsync(int id)
        {
            return await _lANClient.GetGameByIdAsync(id);
        }

        public Task StartGameAsync(int id)
        {
            return _lANClient.StartGameAsync(id);
        }

        public Task CancelGameAsync(int id)
        {
            return _lANClient.CancelGameAsync(id);
        }

        public Task ResetGameAsync(int id)
        {
            return _lANClient.ResetGameAsync(id);
        }

        public Task DeleteGameAsync(int id)
        {
            return _lANClient.DeleteGameAsync(id);
        }
    }
}
