using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Services;

public interface ITeamService
{
    Task<List<Team>> GetTeamsAsync();
    Task<Team> CreateTeamAsync(CreateTeamDTO team);
    Task<Team> UpdateTeamAsync(Guid id, UpdateTeamDTO team);
    Task DeleteTeamAsync(Guid id);
}
