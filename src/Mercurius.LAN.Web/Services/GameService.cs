using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;

namespace Mercurius.LAN.Web.Services
{
    public class GameService : IGameService
    {
        private readonly ILANClient _lanClient;

        public GameService(ILANClient lanClient)
        {
            _lanClient = lanClient;
        }

        public Task<List<Game>> GetGamesAsync()
        {
            return _lanClient.GetGamesAsync();
        }

        public Task<GameExtended?> GetGameByIdAsync(int id)
        {
            return _lanClient.GetGameByIdAsync(id);
        }

        public Task<GameExtended> RegisterForGameAsync(int id, int participantId)
        {
            return _lanClient.RegisterForGameAsync(id, participantId);
        }

        public Task<GameExtended> UnregisterFromGameAsync(int id, int participantId)
        {
            return _lanClient.UnregisterFromGameAsync(id, participantId);
        }
       

        public async Task<GameExtended> CreateGameAsync(CreateGameDTO newGame, string? tempFilePath, string? contentType, string? fileName)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(newGame.Name), "Name" },
                { new StringContent(newGame.BracketType.ToString()), "BracketType" },
                { new StringContent(newGame.Format.ToString()), "Format" },
                { new StringContent(newGame.FinalsFormat.ToString()), "FinalsFormat" },
                { new StringContent(newGame.ParticipantType.ToString()), "ParticipantType" },
                { new StringContent(newGame.RegisterFormUrl.ToString()), "RegisterFormUrl" },
            };

            bool tempFileNeedsCleanup = false;

            if(!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                var fileStream = File.OpenRead(tempFilePath);

                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType!);

                formData.Add(streamContent, "Image", fileName!);

                tempFileNeedsCleanup = true;
            }

            try
            {
                return await _lanClient.CreateGameAsync(formData);
            }
            finally
            {
                if(tempFileNeedsCleanup)
                {
                    try
                    {
                        File.Delete(tempFilePath!);
                    }
                    catch(Exception)
                    {

                    }
                }
            }
        }

        public async Task<Game> UpdateGameAsync(int id, UpdateGameDTO updatedGame, string? tempFilePath,string? contentType, string? fileName)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(updatedGame.Name), "Name" },
                { new StringContent(updatedGame.BracketType.ToString()), "BracketType" },
                { new StringContent(updatedGame.Format.ToString()), "Format" },
                { new StringContent(updatedGame.FinalsFormat.ToString()), "FinalsFormat" },
                { new StringContent(updatedGame.RegisterFormUrl.ToString()), "RegisterFormUrl" },

            };

            bool tempFileNeedsCleanup = false;

            if(!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                var fileStream = File.OpenRead(tempFilePath);

                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType!);

                formData.Add(streamContent, "Image", fileName!);

                tempFileNeedsCleanup = true;
            }

            try
            {
                return await _lanClient.UpdateGameAsync(id, formData);
            }
            finally
            {
                if(tempFileNeedsCleanup)
                {
                    try
                    {
                        File.Delete(tempFilePath!);
                    }
                    catch(Exception)
                    {
                    }
                }
            }
        }

        public Task<GameExtended?> GetGameDetailAsync(int id)
        {
            return _lanClient.GetGameByIdAsync(id);
        }

        public Task StartGameAsync(int id)
        {
            return _lanClient.StartGameAsync(id);
        }

        public Task CancelGameAsync(int id)
        {
            return _lanClient.CancelGameAsync(id);
        }

        public Task ResetGameAsync(int id)
        {
            return _lanClient.ResetGameAsync(id);
        }

        public Task DeleteGameAsync(int id)
        {
            return _lanClient.DeleteGameAsync(id);
        }

        public Task<Match> UpdateMatchScoresAsync(int matchId, UpdateMatchDTO updateMatchDto)
        {
           return _lanClient.UpdateMatchAsync(matchId, updateMatchDto);
        }

        public Task CompleteGameAsync(int id)
        {
            return _lanClient.CompleteGameAsync(id);
        }
    }
}
