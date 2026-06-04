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

        public Task<List<Game>> GetGamesAsync() => _lanClient.GetGamesAsync();

        public Task<GameExtended?> GetGameByIdAsync(Guid id) => _lanClient.GetGameByIdAsync(id);

        public Task<GameExtended> RegisterUserForGameAsync(Guid id, Guid userId) =>
            _lanClient.RegisterUserForGameAsync(id, new RegisterGameUserDTO { UserId = userId });

        public Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId) =>
            _lanClient.UnregisterUserFromGameAsync(id, userId);

        public Task<GameExtended> RegisterTeamForGameAsync(Guid id, Guid teamId) =>
            _lanClient.RegisterTeamForGameAsync(id, new RegisterGameTeamDTO { TeamId = teamId });

        public Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId) =>
            _lanClient.UnregisterTeamFromGameAsync(id, teamId);

        public async Task<GameExtended> CreateGameAsync(CreateGameDTO newGame, string? tempFilePath, string? contentType, string? fileName)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(newGame.Name), "Name" },
                { new StringContent(newGame.BracketType.ToString()), "BracketType" },
                { new StringContent(newGame.Format.ToString()), "Format" },
                { new StringContent(newGame.FinalsFormat.ToString()), "FinalsFormat" },
                { new StringContent(newGame.ParticipationMode.ToString()), "ParticipationMode" },
                { new StringContent(newGame.RegisterFormUrl), "RegisterFormUrl" },
                { new StringContent(SerializeUtcDateTime(newGame.PlannedStartTime)), "PlannedStartTime" },
                { new StringContent(newGame.AverageGameDurationMinutes.ToString()), "AverageGameDurationMinutes" },
                { new StringContent(newGame.RoundBreakDurationMinutes.ToString()), "RoundBreakDurationMinutes" },
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

        public async Task<Game> UpdateGameAsync(Guid id, UpdateGameDTO updatedGame, string? tempFilePath, string? contentType, string? fileName)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(updatedGame.Name), "Name" },
                { new StringContent(updatedGame.BracketType.ToString()), "BracketType" },
                { new StringContent(updatedGame.Format.ToString()), "Format" },
                { new StringContent(updatedGame.FinalsFormat.ToString()), "FinalsFormat" },
                { new StringContent(updatedGame.ParticipationMode.ToString()), "ParticipationMode" },
                { new StringContent(updatedGame.RegisterFormUrl), "RegisterFormUrl" },
                { new StringContent(SerializeUtcDateTime(updatedGame.PlannedStartTime)), "PlannedStartTime" },
                { new StringContent(updatedGame.AverageGameDurationMinutes.ToString()), "AverageGameDurationMinutes" },
                { new StringContent(updatedGame.RoundBreakDurationMinutes.ToString()), "RoundBreakDurationMinutes" },
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

        public Task<GameExtended?> GetGameDetailAsync(Guid id) => _lanClient.GetGameByIdAsync(id);

        private static string SerializeUtcDateTime(DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.ToUniversalTime();

            return utcDateTime.ToString("O");
        }
        public Task StartGameAsync(Guid id) => _lanClient.StartGameAsync(id);
        public Task CancelGameAsync(Guid id) => _lanClient.CancelGameAsync(id);
        public Task ResetGameAsync(Guid id) => _lanClient.ResetGameAsync(id);
        public Task DeleteGameAsync(Guid id) => _lanClient.DeleteGameAsync(id);
        public Task<GameExtended> ReplaceGameSponsorsAsync(Guid id, ReplaceGameSponsorsDTO sponsors) => _lanClient.ReplaceGameSponsorsAsync(id, sponsors);
        public Task<Match> UpdateMatchScoresAsync(Guid matchId, UpdateMatchDTO updateMatchDto) => _lanClient.UpdateMatchAsync(matchId, updateMatchDto);
        public Task CompleteGameAsync(Guid id) => _lanClient.CompleteGameAsync(id);
    }
}
