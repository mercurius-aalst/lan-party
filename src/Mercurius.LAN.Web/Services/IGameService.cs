using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Matches;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;

namespace Mercurius.LAN.Web.Services
{
    public interface IGameService
    {
        Task<List<Game>> GetGamesAsync();
        Task<GameExtended?> GetGameByIdAsync(Guid id);
        Task<GameExtended> CreateGameAsync(CreateGameDTO newGame, string? tempFilePath, string? contentType, string? fileName);
        Task<Game> UpdateGameAsync(Guid id, UpdateGameDTO updatedGame, string? tempFilePath, string? contentType, string? fileName);
        Task<GameExtended?> GetGameDetailAsync(Guid id);
        Task StartGameAsync(Guid id);
        Task CancelGameAsync(Guid id);
        Task ResetGameAsync(Guid id);
        Task DeleteGameAsync(Guid id);
        Task<GameExtended> RegisterUserForGameAsync(Guid id, Guid userId);
        Task<GameExtended> UnregisterUserFromGameAsync(Guid id, Guid userId);
        Task<GameExtended> RegisterTeamForGameAsync(Guid id, Guid teamId);
        Task<GameExtended> UnregisterTeamFromGameAsync(Guid id, Guid teamId);
        Task<Match> UpdateMatchScoresAsync(Guid matchId, UpdateMatchDTO updateMatchDTO);
        Task CompleteGameAsync(Guid id);
    }
}
