using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Services;

public class TeamService : ITeamService
{
    private readonly ILANClient _lanClient;

    public TeamService(ILANClient lanClient)
    {
        _lanClient = lanClient;
    }

    public Task<List<Team>> GetTeamsAsync()
    {
        return _lanClient.GetTeamsAsync();
    }

    public Task<Team> CreateTeamAsync(CreateTeamDTO team)
    {
        return _lanClient.CreateTeamAsync(team);
    }

    public Task<Team> UpdateTeamAsync(Guid id, UpdateTeamDTO team)
    {
        return _lanClient.UpdateTeamAsync(id, team);
    }

    public Task DeleteTeamAsync(Guid id)
    {
        return _lanClient.DeleteTeamAsync(id);
    }
}
