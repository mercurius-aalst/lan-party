using Mercurius.LAN.Web.DTOs.Participants.Teams;

namespace Mercurius.LAN.Web.Services;

public interface ITeamNotificationService
{
    event Func<Task>? Changed;
    IReadOnlyList<TeamNotificationItem> Notifications { get; }
    int UnreadCount { get; }
    Task RefreshAsync(CancellationToken cancellationToken = default);
    Task MarkAllReadAsync();
    Task DismissAsync(string id);
}

public sealed class TeamNotificationService : ITeamNotificationService
{
    private readonly ITeamService _teamService;
    private readonly List<TeamNotificationItem> _notifications = [];
    private readonly HashSet<string> _readIds = [];
    private readonly HashSet<string> _dismissedIds = [];
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    public TeamNotificationService(ITeamService teamService)
    {
        _teamService = teamService;
    }

    public event Func<Task>? Changed;

    public IReadOnlyList<TeamNotificationItem> Notifications => _notifications;

    public int UnreadCount => _notifications.Count(notification => !notification.IsRead);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshGate.WaitAsync(cancellationToken);
        try
        {
            var summary = await _teamService.GetCurrentUserTeamSummaryAsync(cancellationToken);
            _notifications.Clear();

            foreach(var invite in summary.ReceivedPendingInvites.OrderByDescending(invite => invite.CreatedAt))
            {
                var id = $"team-invite:{invite.Id:N}";
                if(_dismissedIds.Contains(id))
                    continue;

                _notifications.Add(new TeamNotificationItem(
                    id,
                    "Team invite",
                    $"You have a pending invite to {invite.TeamName}.",
                    "/teams/manage#received-invites",
                    invite.TeamId,
                    invite.Id,
                    _readIds.Contains(id),
                    invite.CreatedAt));
            }
        }
        finally
        {
            _refreshGate.Release();
        }

        await NotifyChangedAsync();
    }

    public async Task MarkAllReadAsync()
    {
        foreach(var notification in _notifications)
        {
            _readIds.Add(notification.Id);
            notification.IsRead = true;
        }

        await NotifyChangedAsync();
    }

    public async Task DismissAsync(string id)
    {
        _dismissedIds.Add(id);
        _notifications.RemoveAll(notification => notification.Id == id);
        await NotifyChangedAsync();
    }

    private async Task NotifyChangedAsync()
    {
        var handler = Changed;
        if(handler != null)
            await handler();
    }
}

public sealed record TeamNotificationItem(
    string Id,
    string Title,
    string Message,
    string Href,
    Guid? TeamId,
    Guid? InviteId,
    bool IsRead,
    DateTime CreatedAt)
{
    public bool IsRead { get; set; } = IsRead;
}
