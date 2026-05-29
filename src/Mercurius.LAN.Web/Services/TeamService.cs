using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Models.Participants;
using Refit;
using System.Net;

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

    public async Task<PublicTeamProfileDTO?> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
    {
        var trimmedTeamName = teamName.Trim();
        if(string.IsNullOrWhiteSpace(trimmedTeamName))
            return null;

        try
        {
            return await _lanClient.GetPublicTeamByNameAsync(trimmedTeamName, cancellationToken);
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
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
