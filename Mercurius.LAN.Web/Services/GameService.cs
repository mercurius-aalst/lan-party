using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models;

namespace Mercurius.LAN.Web.Services
{
    public class GameService : IGameService
    {
        private readonly ILANClient _lANClient;

        public GameService(ILANClient lANClient)
        {
            _lANClient = lANClient;
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            return await _lANClient.GetGamesAsync();
        }

        public async Task<GameExtended?> GetGameByIdAsync(int id)
        {
            return await _lANClient.GetGameByIdAsync(id);
        }

        public Task<Game> RegisterForGameAsync() => throw new NotImplementedException();
    }
}
