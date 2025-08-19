using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using System.Net.Http;

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
       

        public async Task<GameExtended> CreateGameAsync(CreateGameDTO newGame)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(newGame.Name), "Name" },
                { new StringContent(newGame.BracketType.ToString()), "BracketType" },
                { new StringContent(newGame.Format.ToString()), "Format" },
                { new StringContent(newGame.FinalsFormat.ToString()), "FinalsFormat" }
            };

            if (newGame.Image != null)
            {
                var streamContent = new StreamContent(newGame.Image.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(newGame.Image.ContentType);
                formData.Add(streamContent, "Image", newGame.Image.Name);
            }

            return await _lANClient.CreateGameAsync(formData);
        }

        public async Task<Game> UpdateGameAsync(int id, UpdateGameDTO updatedGame)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(updatedGame.Name), "Name" },
                { new StringContent(updatedGame.BracketType.ToString()), "BracketType" },
                { new StringContent(updatedGame.Format.ToString()), "Format" },
                { new StringContent(updatedGame.FinalsFormat.ToString()), "FinalsFormat" }
            };

            if (updatedGame.Image != null)
            {
                var streamContent = new StreamContent(updatedGame.Image.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(updatedGame.Image.ContentType);
                formData.Add(streamContent, "Image", updatedGame.Image.Name);
            }

            return await _lANClient.UpdateGameAsync(id, formData);
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

        public Task<Match> UpdateMatchScoresAsync(int matchId, UpdateMatchDTO updateMatchDTO)
        {
           return _lANClient.UpdateMatchAsync(matchId, updateMatchDTO);
        }

        public Task CompleteGameAsync(int id)
        {
            return _lANClient.CompleteGameAsync(id);
        }
    }
}
