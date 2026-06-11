using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Participants;
using Refit;
using System.Net;

namespace Mercurius.LAN.Web.Services;

public class TeamService : ITeamService
{
    private readonly ILANClient _lanClient;
    private readonly IConfiguration _configuration;

    public TeamService(ILANClient lanClient, IConfiguration configuration)
    {
        _lanClient = lanClient;
        _configuration = configuration;
    }

    public async Task<List<Team>> GetTeamsAsync()
    {
        try
        {
            var teams = await _lanClient.GetTeamsAsync();
            teams.ForEach(team => TeamAssetUrlResolver.Resolve(_configuration, team));
            return teams;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Load teams", exception);
        }
    }

    public async Task<PublicTeamProfileDTO?> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
    {
        var trimmedTeamName = teamName.Trim();
        if(string.IsNullOrWhiteSpace(trimmedTeamName))
            return null;

        try
        {
            var team = await _lanClient.GetPublicTeamByNameAsync(trimmedTeamName, cancellationToken);
            if(team is not null)
                TeamAssetUrlResolver.Resolve(_configuration, team);

            return team;
        }
        catch(ApiException exception) when(exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Load public team profile", exception);
        }
    }

    public async Task<CurrentUserTeamSummaryDTO> GetCurrentUserTeamSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _lanClient.GetCurrentUserTeamSummaryAsync(cancellationToken);
            TeamAssetUrlResolver.Resolve(_configuration, summary);
            return summary;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Load team summary", exception);
        }
    }

    public async Task<Team> CreateTeamAsync(CreateTeamDTO team)
    {
        try
        {
            var createdTeam = await _lanClient.CreateTeamAsync(team);
            TeamAssetUrlResolver.Resolve(_configuration, createdTeam);
            return createdTeam;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Create team", exception);
        }
    }

    public async Task<TeamInvite> InviteUserAsync(Guid teamId, Guid userId)
    {
        try
        {
            return await _lanClient.CreateTeamInviteAsync(teamId, userId);
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Invite user to team", exception);
        }
    }

    public async Task<TeamInvite> CancelInviteAsync(Guid teamId, Guid inviteId)
    {
        try
        {
            return await _lanClient.CancelTeamInviteAsync(teamId, inviteId);
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Cancel team invite", exception);
        }
    }

    public async Task<TeamInvite> RespondToInviteAsync(Guid inviteId, bool accept)
    {
        try
        {
            return await _lanClient.RespondToCurrentUserTeamInviteAsync(inviteId, new RespondTeamInviteDTO { Accept = accept });
        }
        catch(ApiException exception)
        {
            var operation = accept ? "Accept team invite" : "Decline team invite";
            throw CreateServiceException(operation, exception);
        }
    }

    public async Task<TeamManagementSummaryDTO> LeaveTeamAsync(Guid teamId)
    {
        try
        {
            var team = await _lanClient.LeaveTeamAsync(teamId);
            TeamAssetUrlResolver.Resolve(_configuration, team);
            return team;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Leave team", exception);
        }
    }

    public async Task<TeamManagementSummaryDTO> RemoveMemberAsync(Guid teamId, Guid userId)
    {
        try
        {
            var team = await _lanClient.RemoveTeamMemberAsync(teamId, userId);
            TeamAssetUrlResolver.Resolve(_configuration, team);
            return team;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Remove team member", exception);
        }
    }

    public async Task<TeamManagementSummaryDTO> TransferCaptainAsync(Guid teamId, Guid newCaptainUserId)
    {
        try
        {
            var team = await _lanClient.TransferTeamCaptainAsync(teamId, new TransferCaptainDTO { NewCaptainUserId = newCaptainUserId });
            TeamAssetUrlResolver.Resolve(_configuration, team);
            return team;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Transfer team captain", exception);
        }
    }

    public async Task<TeamLogoResponseDTO> UploadLogoAsync(Guid teamId, Stream logoStream, string contentType, string fileName)
    {
        var logo = new StreamPart(logoStream, fileName, contentType);
        try
        {
            var response = await _lanClient.UploadTeamLogoAsync(teamId, logo);
            TeamAssetUrlResolver.Resolve(_configuration, response);
            return response;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Upload team logo", exception);
        }
    }

    public async Task<TeamLogoResponseDTO> RemoveLogoAsync(Guid teamId)
    {
        try
        {
            var response = await _lanClient.RemoveTeamLogoAsync(teamId);
            TeamAssetUrlResolver.Resolve(_configuration, response);
            return response;
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Remove team logo", exception);
        }
    }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        try
        {
            await _lanClient.DeleteTeamAsync(teamId);
        }
        catch(ApiException exception)
        {
            throw CreateServiceException("Delete team", exception);
        }
    }

    private static TeamServiceException CreateServiceException(string operation, ApiException exception)
    {
        var message = GetUserFacingMessage(exception);
        var status = exception.StatusCode == 0
            ? "unknown status"
            : $"{(int)exception.StatusCode} {exception.StatusCode}";

        return new TeamServiceException($"{operation} failed ({status}): {message}", operation, exception.StatusCode, exception.Content, exception);
    }

    private static string GetUserFacingMessage(ApiException exception)
    {
        if(!string.IsNullOrWhiteSpace(exception.Content))
            return exception.Content.Trim();

        return exception.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "You need to sign in before managing teams.",
            HttpStatusCode.Forbidden => "You are not allowed to perform this team action.",
            HttpStatusCode.NotFound => "That team or invite could not be found.",
            _ => "The team action could not be completed right now."
        };
    }
}

public sealed class TeamServiceException : Exception
{
    public TeamServiceException(string message, string operation, HttpStatusCode statusCode, string? apiContent, Exception innerException) : base(message, innerException)
    {
        Operation = operation;
        StatusCode = statusCode;
        ApiContent = apiContent;
    }

    public string Operation { get; }
    public HttpStatusCode StatusCode { get; }
    public string? ApiContent { get; }
}
