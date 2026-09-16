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
    // RefreshAsync runs on the SignalR callback thread while dismiss/read mutations run on the
    // circuit thread, so every access to the notification state is guarded by this lock.
    private readonly object _stateLock = new();
    private TeamNotificationItem[] _snapshot = [];

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

    public IReadOnlyList<TeamNotificationItem> Notifications => _snapshot;

    public int UnreadCount => _snapshot.Count(notification => !notification.IsRead);

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshGate.WaitAsync(cancellationToken);
        try
        {
            var summaryTask = LoadTeamSummaryAsync();
            var rosterTask = LoadRosterConfirmationsAsync();

            try
            {
                await Task.WhenAll(summaryTask, rosterTask);
            }
            catch
            {
                cancellationToken.ThrowIfCancellationRequested();

                if(summaryTask.IsCanceled)
                    await summaryTask;
                if(rosterTask.IsCanceled)
                    await rosterTask;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var summaryFailure = GetFailure(summaryTask);
            var rosterFailure = GetFailure(rosterTask);

            if(summaryFailure is null && summaryTask.Status == TaskStatus.RanToCompletion)
                ReplaceNotifications(TeamNotificationKind.TeamInvite, CreateTeamInviteNotifications(await summaryTask));

            if(rosterFailure is null && rosterTask.Status == TaskStatus.RanToCompletion)
                ReplaceNotifications(TeamNotificationKind.RosterSelection, CreateRosterNotifications(await rosterTask));

            if(summaryFailure is not null && rosterFailure is not null)
            {
                throw new AggregateException(
                    "Could not refresh team and roster notifications.",
                    summaryFailure,
                    rosterFailure);
            }
        }
        finally
        {
            _refreshGate.Release();
        }

        await NotifyChangedAsync();

        async Task<CurrentUserTeamSummaryDTO> LoadTeamSummaryAsync() =>
            await _teamService.GetCurrentUserTeamSummaryAsync(cancellationToken);

        async Task<IReadOnlyList<PendingRosterConfirmationDTO>> LoadRosterConfirmationsAsync() =>
            await _tournamentService.GetPendingRosterConfirmationsAsync(cancellationToken);
    }

    public async Task RespondToRosterSelectionAsync(
        string notificationId,
        bool accept,
        CancellationToken cancellationToken = default)
    {
        TeamNotificationItem? notification;
        lock(_stateLock)
        {
            notification = _notifications.FirstOrDefault(item =>
                item.Id == notificationId && item.Kind == TeamNotificationKind.RosterSelection);
        }

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
        catch(OperationCanceledException)
        {
            throw;
        }
        catch(Exception)
        {
            // The mutation already succeeded. Keep it successful even if the follow-up read is unavailable.
        }
        finally
        {
            await NotifyRosterDecisionChangedAsync(tournamentId);
        }
    }

    public async Task MarkAllReadAsync()
    {
        lock(_stateLock)
        {
            foreach(var notification in _notifications)
            {
                _readIds.Add(notification.Id);
                notification.IsRead = true;
            }

            _snapshot = _notifications.ToArray();
        }

        await NotifyChangedAsync();
    }

    public async Task DismissAsync(string id)
    {
        lock(_stateLock)
        {
            _dismissedIds.Add(id);
            _notifications.RemoveAll(notification => notification.Id == id);
            _snapshot = _notifications.ToArray();
        }

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

    private void ReplaceNotifications(
        TeamNotificationKind kind,
        IEnumerable<TeamNotificationItem> notifications)
    {
        lock(_stateLock)
        {
            // The sequence reads the dismissed/read id sets, so it is materialised under the
            // same lock that guards those sets.
            _notifications.RemoveAll(notification => notification.Kind == kind);
            _notifications.AddRange(notifications);
            _notifications.Sort((left, right) => right.CreatedAt.CompareTo(left.CreatedAt));
            _snapshot = _notifications.ToArray();
        }
    }

    private IEnumerable<TeamNotificationItem> CreateTeamInviteNotifications(CurrentUserTeamSummaryDTO summary)
    {
        foreach(var invite in summary.ReceivedPendingInvites.OrderByDescending(invite => invite.CreatedAt))
        {
            var id = $"team-invite:{invite.Id:N}";
            if(_dismissedIds.Contains(id))
                continue;

            yield return new TeamNotificationItem(
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
                invite.CreatedAt);
        }
    }

    private IEnumerable<TeamNotificationItem> CreateRosterNotifications(
        IReadOnlyList<PendingRosterConfirmationDTO> rosterConfirmations)
    {
        foreach(var roster in rosterConfirmations)
        {
            var id = $"roster-selection:{roster.RosterMemberId:N}";
            if(_dismissedIds.Contains(id))
                continue;

            yield return new TeamNotificationItem(
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
                roster.SelectedAtUtc);
        }
    }

    private static Exception? GetFailure<T>(Task<T> task) =>
        task.IsFaulted ? task.Exception?.GetBaseException() : null;
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
