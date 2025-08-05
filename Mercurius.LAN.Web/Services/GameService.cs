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

        public Task<List<Game>> GetGamesAsync()
        {
            return _lANClient.GetGamesAsync();

        }

        public Task<GameExtended?> GetGameByIdAsync(int id)
        {
            return _lANClient.GetGameByIdAsync(id);
        }

        public Task<Game> RegisterForGameAsync() => throw new NotImplementedException();
    }
}
