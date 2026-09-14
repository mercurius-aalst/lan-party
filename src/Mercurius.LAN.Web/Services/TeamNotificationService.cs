using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.Registrations;
using Mercurius.LAN.Web.Localization;

namespace Mercurius.LAN.Web.Services;

public interface ITeamNotificationService
{
    event Func<Task>? Changed;
    event Func<Guid, Task>? RosterDecisionChanged;
    IReadOnlyList<TeamNotificationItem> Notifications { get; }
    int UnreadCount { get; }
    Task RefreshAsync(CancellationToken cancellationToken = default);
    Task RespondToRosterSelectionAsync(string notificationId, bool accept, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync();
    Task DismissAsync(string id);
}

public sealed class TeamNotificationService : ITeamNotificationService
{
    private readonly ITeamService _teamService;
    private readonly ITournamentService _tournamentService;
    private readonly ILocalizationService _localization;
    private readonly List<TeamNotificationItem> _notifications = [];
    private readonly HashSet<string> _readIds = [];
    private readonly HashSet<string> _dismissedIds = [];
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    public TeamNotificationService(
        ITeamService teamService,
        ITournamentService tournamentService,
        ILocalizationService localization)
    {
        _teamService = teamService;
        _tournamentService = tournamentService;
        _localization = localization;
    }

    public event Func<Task>? Changed;
    public event Func<Guid, Task>? RosterDecisionChanged;

    public IReadOnlyList<TeamNotificationItem> Notifications => _notifications;

    public int UnreadCount => _notifications.Count(notification => !notification.IsRead);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshGate.WaitAsync(cancellationToken);
        try
        {
            var summaryTask = _teamService.GetCurrentUserTeamSummaryAsync(cancellationToken);
            var rosterTask = _tournamentService.GetPendingRosterConfirmationsAsync(cancellationToken);
            await Task.WhenAll(summaryTask, rosterTask);
            var summary = await summaryTask;
            var rosterConfirmations = await rosterTask;
            _notifications.Clear();

            foreach(var invite in summary.ReceivedPendingInvites.OrderByDescending(invite => invite.CreatedAt))
            {
                var id = $"team-invite:{invite.Id:N}";
                if(_dismissedIds.Contains(id))
                    continue;

                _notifications.Add(new TeamNotificationItem(
                    id,
                    TeamNotificationKind.TeamInvite,
                    _localization["nav.teamInviteTitle"],
                    _localization.Get("nav.teamInviteMessage", invite.TeamName),
                    "/teams/manage#received-invites",
                    invite.TeamId,
                    invite.Id,
                    null,
                    null,
                    _readIds.Contains(id),
                    invite.CreatedAt));
            }

            foreach(var roster in rosterConfirmations)
            {
                var id = $"roster-selection:{roster.RosterMemberId:N}";
                if(_dismissedIds.Contains(id))
                    continue;

                _notifications.Add(new TeamNotificationItem(
                    id,
                    TeamNotificationKind.RosterSelection,
                    _localization["nav.rosterSelectionTitle"],
                    _localization.Get("nav.rosterSelectionMessage", roster.TeamName, roster.TournamentName),
                    $"/tournaments/{roster.TournamentId}#registration",
                    roster.TeamId,
                    null,
                    roster.TournamentId,
                    roster.RosterMemberId,
                    _readIds.Contains(id),
                    roster.SelectedAtUtc));
            }

            _notifications.Sort((left, right) => right.CreatedAt.CompareTo(left.CreatedAt));
        }
        finally
        {
            _refreshGate.Release();
        }

        await NotifyChangedAsync();
    }

    public async Task RespondToRosterSelectionAsync(
        string notificationId,
        bool accept,
        CancellationToken cancellationToken = default)
    {
        var notification = _notifications.FirstOrDefault(item =>
            item.Id == notificationId && item.Kind == TeamNotificationKind.RosterSelection);
        if(notification?.TournamentId is not Guid tournamentId ||
           notification.RosterMemberId is not Guid rosterMemberId)
        {
            await RefreshAsync(cancellationToken);
            return;
        }

        if(accept)
            await _tournamentService.ConfirmTournamentRosterMemberAsync(tournamentId, rosterMemberId, cancellationToken);
        else
            await _tournamentService.DeclineTournamentRosterMemberAsync(tournamentId, rosterMemberId, cancellationToken);

        try
        {
            await RefreshAsync(cancellationToken);
        }
        finally
        {
            await NotifyRosterDecisionChangedAsync(tournamentId);
        }
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

    private async Task NotifyRosterDecisionChangedAsync(Guid tournamentId)
    {
        var handler = RosterDecisionChanged;
        if(handler != null)
            await handler(tournamentId);
    }
}

public sealed record TeamNotificationItem(
    string Id,
    TeamNotificationKind Kind,
    string Title,
    string Message,
    string Href,
    Guid? TeamId,
    Guid? InviteId,
    Guid? TournamentId,
    Guid? RosterMemberId,
    bool IsRead,
    DateTime CreatedAt)
{
    public bool IsRead { get; set; } = IsRead;
}

public enum TeamNotificationKind
{
    TeamInvite,
    RosterSelection
}
