using Mercurius.LAN.Web.DTOs.Participants.Players;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Services
{
    public interface IParticipantService
    {
        Task<List<Player>> GetPlayersAsync();
        Task<Player> CreatePlayerAsync(CreatePlayerDTO player);
        Task<Player> UpdatePlayerAsync(int id, UpdatePlayerDTO player);
        Task DeletePlayerAsync(int id);

        Task<List<Team>> GetTeamsAsync();
        Task<Team> CreateTeamAsync(CreateTeamDTO team);
        Task<Team> UpdateTeamAsync(int id, UpdateTeamDTO team);
        Task DeleteTeamAsync(int id);
    }
}