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

        public Task<Game> RegisterForGameAsync() => throw new NotImplementedException();

        public async Task<GameExtended> CreateGameAsync(CreateGameDTO newGame)
        {
            return await _lANClient.CreateGameAsync(newGame);
        }
    }
}
