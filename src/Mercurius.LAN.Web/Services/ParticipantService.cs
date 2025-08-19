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

        public Task<List<Player>> GetPlayersAsync()
        {
            return _lanClient.GetPlayersAsync();
        }

        public Task<Player> CreatePlayerAsync(CreatePlayerDTO player)
        {
            return _lanClient.CreatePlayerAsync(player);
        }

        public Task<Player> UpdatePlayerAsync(int id, UpdatePlayerDTO player)
        {
            return _lanClient.UpdatePlayerAsync(id, player);
        }

        public Task DeletePlayerAsync(int id)
        {
            return _lanClient.DeletePlayerAsync(id);
        }

        public Task<List<Team>> GetTeamsAsync()
        {
            return _lanClient.GetTeamsAsync();
        }

        public Task<Team> CreateTeamAsync(CreateTeamDTO team)
        {
            return _lanClient.CreateTeamAsync(team);
        }

        public Task<Team> UpdateTeamAsync(int id, UpdateTeamDTO team)
        {
            return _lanClient.UpdateTeamAsync(id, team);
        }

        public Task DeleteTeamAsync(int id)
        {
            return _lanClient.DeleteTeamAsync(id);
        }
    }
}