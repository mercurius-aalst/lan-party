using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Teams;

public partial class PublicTeamProfile
    : IDisposable
{
    [Inject] private ITeamService TeamService { get; set; } = null!;

    [Parameter] public string TeamName { get; set; } = string.Empty;

    private PublicTeamProfileDTO? _team;
    private PublicProfileMatchSummariesDTO? _matchSummaries;
    private bool _isLoading;
    private bool _hasError;
    private bool _isMatchSummariesLoading;
    private bool _hasMatchSummariesError;
    private CancellationTokenSource? _loadCancellation;
    private long _loadGeneration;
    private bool _disposed;
    private string? _loadedTeamName;
    private int MemberCount => _team?.Members.Count ?? 0;

    protected override async Task OnParametersSetAsync()
    {
        if(_disposed)
            return;

        var generation = ++_loadGeneration;
        CancelCurrentLoad();
        using var cancellation = new CancellationTokenSource();
        _loadCancellation = cancellation;
        var cancellationToken = cancellation.Token;

        _isLoading = true;
        _hasError = false;
        _team = null;
        _loadedTeamName = null;
        _matchSummaries = null;
        _isMatchSummariesLoading = false;
        _hasMatchSummariesError = false;

        var decodedTeamName = Uri.UnescapeDataString(TeamName ?? string.Empty).Trim();
        try
        {
            if(string.IsNullOrWhiteSpace(decodedTeamName))
                return;

            try
            {
                var team = await TeamService.GetPublicTeamByNameAsync(decodedTeamName, cancellationToken);
                if(!IsCurrentLoad(generation, cancellationToken))
                    return;

                _team = team;
                _loadedTeamName = decodedTeamName;
            }
            catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch(Exception)
            {
                if(IsCurrentLoad(generation, cancellationToken))
                    _hasError = true;
            }
            finally
            {
                if(IsCurrentLoad(generation, cancellationToken))
                    _isLoading = false;
            }

            if(!IsCurrentLoad(generation, cancellationToken) || _hasError || _team is null)
                return;

            await LoadMatchSummariesAsync(decodedTeamName, generation, cancellationToken);
        }
        finally
        {
            if(ReferenceEquals(_loadCancellation, cancellation))
                _loadCancellation = null;
        }
    }

    private async Task LoadMatchSummariesAsync(
        string teamName,
        long generation,
        CancellationToken cancellationToken)
    {
        if(!IsCurrentLoad(generation, cancellationToken))
            return;

        _isMatchSummariesLoading = true;
        await NotifyStateChangedAsync(generation, cancellationToken);
        if(!IsCurrentLoad(generation, cancellationToken))
            return;

        try
        {
            var matchSummaries = await TeamService.GetPublicTeamMatchSummariesAsync(teamName, cancellationToken);
            if(!IsCurrentLoad(generation, cancellationToken))
                return;

            _matchSummaries = matchSummaries ?? new PublicProfileMatchSummariesDTO();
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch(Exception)
        {
            if(IsCurrentLoad(generation, cancellationToken))
                _hasMatchSummariesError = true;
        }
        finally
        {
            if(IsCurrentLoad(generation, cancellationToken))
                _isMatchSummariesLoading = false;
        }
    }

    private async Task RetryMatchSummariesAsync()
    {
        if(_disposed || _team is null || string.IsNullOrWhiteSpace(_loadedTeamName))
            return;

        var generation = ++_loadGeneration;
        CancelCurrentLoad();
        using var cancellation = new CancellationTokenSource();
        _loadCancellation = cancellation;
        _matchSummaries = null;
        _hasMatchSummariesError = false;

        try
        {
            await LoadMatchSummariesAsync(_loadedTeamName, generation, cancellation.Token);
        }
        finally
        {
            if(ReferenceEquals(_loadCancellation, cancellation))
                _loadCancellation = null;
        }
    }

    public void Dispose()
    {
        _disposed = true;
        ++_loadGeneration;
        CancelCurrentLoad();
        _loadCancellation = null;
    }

    private void CancelCurrentLoad()
    {
        try
        {
            _loadCancellation?.Cancel();
        }
        catch(ObjectDisposedException)
        {
            // The owning async load disposes its source after it completes.
        }
    }

    private bool IsCurrentLoad(long generation, CancellationToken cancellationToken) =>
        !_disposed && generation == _loadGeneration && !cancellationToken.IsCancellationRequested;

    private async Task NotifyStateChangedAsync(long generation, CancellationToken cancellationToken)
    {
        if(!IsCurrentLoad(generation, cancellationToken))
            return;

        try
        {
            await InvokeAsync(StateHasChanged);
        }
        catch(ObjectDisposedException) when(!IsCurrentLoad(generation, cancellationToken))
        {
        }
        catch(InvalidOperationException) when(!IsCurrentLoad(generation, cancellationToken))
        {
        }
    }

    private static string BuildMemberProfileHref(string username) =>
        $"/users/{Uri.EscapeDataString(username)}";

    private static string GetMemberInitials(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return "?";

        var trimmed = username.Trim();
        if(trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return trimmed[..2].ToUpperInvariant();
    }
}
