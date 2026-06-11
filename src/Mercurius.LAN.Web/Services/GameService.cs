using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;

namespace Mercurius.LAN.Web.Services
{
    public class GameService : IGameService
    {
        private readonly ILANClient _lanClient;
        private readonly IConfiguration _configuration;

        public GameService(ILANClient lanClient, IConfiguration configuration)
        {
            _lanClient = lanClient;
            _configuration = configuration;
        }

        public Task<List<Game>> GetGamesAsync() => _lanClient.GetGamesAsync();

        public async Task<GameExtended?> GetGameByIdAsync(Guid id)
        {
            var game = await _lanClient.GetGameByIdAsync(id);
            if(game is not null)
                TeamAssetUrlResolver.Resolve(_configuration, game);

            return game;
        }

        public async Task<GameExtended> RegisterUserForGameAsync(Guid id, Guid userId) =>
            ResolveGameAssetUrls(await _lanClient.RegisterUserForGameAsync(id, new RegisterGameUserDTO { UserId = userId }));

        public async Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId) =>
            ResolveGameAssetUrls(await _lanClient.UnregisterUserFromGameAsync(id, userId));

        public async Task<GameExtended> RegisterTeamForGameAsync(Guid id, Guid teamId) =>
            ResolveGameAssetUrls(await _lanClient.RegisterTeamForGameAsync(id, new RegisterGameTeamDTO { TeamId = teamId }));

        public async Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId) =>
            ResolveGameAssetUrls(await _lanClient.UnregisterTeamFromGameAsync(id, teamId));

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
                { new StringContent(newGame.PlannedStartTime.ToUtcIsoString()), "PlannedStartTime" },
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
                return ResolveGameAssetUrls(await _lanClient.CreateGameAsync(formData));
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

        public async Task<GameExtended> UpdateGameAsync(Guid id, UpdateGameDTO updatedGame, string? tempFilePath, string? contentType, string? fileName)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(updatedGame.Name), "Name" },
                { new StringContent(updatedGame.BracketType.ToString()), "BracketType" },
                { new StringContent(updatedGame.Format.ToString()), "Format" },
                { new StringContent(updatedGame.FinalsFormat.ToString()), "FinalsFormat" },
                { new StringContent(updatedGame.ParticipationMode.ToString()), "ParticipationMode" },
                { new StringContent(updatedGame.RegisterFormUrl), "RegisterFormUrl" },
                { new StringContent(updatedGame.PlannedStartTime.ToUtcIsoString()), "PlannedStartTime" },
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
                return ResolveGameAssetUrls(await _lanClient.UpdateGameAsync(id, formData));
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

        public Task<GameExtended?> GetGameDetailAsync(Guid id) => GetGameByIdAsync(id);

        public Task StartGameAsync(Guid id) => _lanClient.StartGameAsync(id);
        public Task CancelGameAsync(Guid id) => _lanClient.CancelGameAsync(id);
        public Task ResetGameAsync(Guid id) => _lanClient.ResetGameAsync(id);
        public Task DeleteGameAsync(Guid id) => _lanClient.DeleteGameAsync(id);
        public async Task<GameExtended> ReplaceGameSponsorsAsync(Guid id, ReplaceGameSponsorsDTO sponsors) =>
            ResolveGameAssetUrls(await _lanClient.ReplaceGameSponsorsAsync(id, sponsors));
        public Task<Match> UpdateMatchScoresAsync(Guid matchId, UpdateMatchDTO updateMatchDto) => _lanClient.UpdateMatchAsync(matchId, updateMatchDto);
        public Task CompleteGameAsync(Guid id) => _lanClient.CompleteGameAsync(id);

        private GameExtended ResolveGameAssetUrls(GameExtended game)
        {
            TeamAssetUrlResolver.Resolve(_configuration, game);
            return game;
        }
    }
}
