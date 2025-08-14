using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Participants.Players;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Models.Participants;
using Refit;

namespace Mercurius.LAN.Web.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly ILANClient _lanClient;

        public ParticipantService(ILANClient lanClient)
        {
            _lanClient = lanClient;
        }

        public async Task<List<Player>> GetPlayersAsync()
        {
            return await _lanClient.GetPlayersAsync();
        }

        public async Task<Player> CreatePlayerAsync(CreatePlayerDTO player)
        {
            return await _lanClient.CreatePlayerAsync(player);
        }

        public async Task<Player> UpdatePlayerAsync(int id, UpdatePlayerDTO player)
        {
            return await _lanClient.UpdatePlayerAsync(id, player);
        }

        public async Task DeletePlayerAsync(int id)
        {
            await _lanClient.DeletePlayerAsync(id);
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            return await _lanClient.GetTeamsAsync();
        }

        public async Task<Team> CreateTeamAsync(CreateTeamDTO team)
        {
            return await _lanClient.CreateTeamAsync(team);
        }

        public async Task<Team> UpdateTeamAsync(int id, UpdateTeamDTO team)
        {
            return await _lanClient.UpdateTeamAsync(id, team);
        }

        public async Task DeleteTeamAsync(int id)
        {
            await _lanClient.DeleteTeamAsync(id);
        }
    }
}