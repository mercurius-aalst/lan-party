using Mercurius.LAN.Web.DTOs.Users;

namespace Mercurius.LAN.Web.DTOs.Participants.Teams;

public sealed class CurrentUserTeamSummaryDTO
{
    public IReadOnlyList<TeamManagementSummaryDTO> CaptainedTeams { get; set; } = [];
    public IReadOnlyList<TeamManagementSummaryDTO> MemberTeams { get; set; } = [];
    public IReadOnlyList<TeamInviteSummaryDTO> ReceivedPendingInvites { get; set; } = [];
    public IReadOnlyList<TeamInviteSummaryDTO> SentPendingInvites { get; set; } = [];
}

public sealed class TeamManagementSummaryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CaptainUserId { get; set; }
    public string? CaptainUsername { get; set; }
    public string? LogoUrl { get; set; }
    public IReadOnlyList<PublicUserDTO> Members { get; set; } = [];
}

public sealed class TeamInviteSummaryDTO
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogoUrl { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public sealed class TransferCaptainDTO
{
    public Guid NewCaptainUserId { get; set; }
}

public sealed class TeamLogoResponseDTO
{
    public Guid TeamId { get; set; }
    public string? LogoUrl { get; set; }
}

public sealed record TeamInviteChangedEvent(Guid TeamId, Guid InviteId, Guid UserId, string Status);

public sealed record TeamMembershipChangedEvent(Guid TeamId, Guid UserId, string Action);

public sealed record TeamCaptainTransferredEvent(Guid TeamId, Guid NewCaptainUserId);
