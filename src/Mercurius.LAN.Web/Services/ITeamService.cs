using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Services;

public interface ITeamService
{
    Task<List<Team>> GetTeamsAsync();
    Task<PublicTeamProfileDTO?> GetPublicTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);
    Task<CurrentUserTeamSummaryDTO> GetCurrentUserTeamSummaryAsync(CancellationToken cancellationToken = default);
    Task<Team> CreateTeamAsync(CreateTeamDTO team);
    Task<TeamInvite> InviteUserAsync(Guid teamId, Guid userId);
    Task<TeamInvite> CancelInviteAsync(Guid teamId, Guid inviteId);
    Task<TeamInvite> RespondToInviteAsync(Guid inviteId, bool accept);
    Task<TeamManagementSummaryDTO> LeaveTeamAsync(Guid teamId);
    Task<TeamManagementSummaryDTO> RemoveMemberAsync(Guid teamId, Guid userId);
    Task<TeamManagementSummaryDTO> TransferCaptainAsync(Guid teamId, Guid newCaptainUserId);
    Task<TeamLogoResponseDTO> UploadLogoAsync(Guid teamId, Stream logoStream, string contentType, string fileName);
    Task<TeamLogoResponseDTO> RemoveLogoAsync(Guid teamId);
    Task DeleteTeamAsync(Guid teamId);
}
