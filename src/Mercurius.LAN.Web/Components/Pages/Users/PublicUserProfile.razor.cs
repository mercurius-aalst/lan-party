using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class PublicUserProfile
    : IDisposable
{
    [Inject] private IPublicProfileService PublicProfileService { get; set; } = null!;

    [Parameter] public string Username { get; set; } = string.Empty;

    private PublicUserProfileDTO? _profile;
    private PublicProfileMatchSummariesDTO? _matchSummaries;
    private bool _isLoading;
    private bool _hasError;
    private bool _isMatchSummariesLoading;
    private bool _hasMatchSummariesError;
    private CancellationTokenSource? _loadCancellation;
    private long _loadGeneration;
    private bool _disposed;
    private string? _loadedUsername;
    private bool HasLinkedIdentities =>
        !string.IsNullOrWhiteSpace(_profile?.DiscordId) ||
        !string.IsNullOrWhiteSpace(_profile?.SteamId) ||
        !string.IsNullOrWhiteSpace(_profile?.RiotId);
    private string FullName => GetFullName(_profile);
    private string PageTitleText => _profile is null ? "User Profile" : $"{FullName} Profile";

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
        _profile = null;
        _loadedUsername = null;
        _matchSummaries = null;
        _isMatchSummariesLoading = false;
        _hasMatchSummariesError = false;

        var decodedUsername = Uri.UnescapeDataString(Username ?? string.Empty).Trim();
        try
        {
            if(string.IsNullOrWhiteSpace(decodedUsername))
                return;

            try
            {
                var profile = await PublicProfileService.GetPublicUserByUsernameAsync(decodedUsername, cancellationToken);
                if(!IsCurrentLoad(generation, cancellationToken))
                    return;

                _profile = profile;
                _loadedUsername = decodedUsername;
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

            if(!IsCurrentLoad(generation, cancellationToken) || _hasError || _profile is null)
                return;

            await LoadMatchSummariesAsync(decodedUsername, generation, cancellationToken);
        }
        finally
        {
            if(ReferenceEquals(_loadCancellation, cancellation))
                _loadCancellation = null;
        }
    }

    private async Task LoadMatchSummariesAsync(
        string username,
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
            var matchSummaries = await PublicProfileService.GetPublicUserMatchSummariesAsync(username, cancellationToken);
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
        if(_disposed || _profile is null || string.IsNullOrWhiteSpace(_loadedUsername))
            return;

        var generation = ++_loadGeneration;
        CancelCurrentLoad();
        using var cancellation = new CancellationTokenSource();
        _loadCancellation = cancellation;
        _matchSummaries = null;
        _hasMatchSummariesError = false;

        try
        {
            await LoadMatchSummariesAsync(_loadedUsername, generation, cancellation.Token);
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

    private static string GetFullName(PublicUserProfileDTO? profile)
    {
        if(profile is null)
            return string.Empty;

        var fullName = $"{profile.Firstname} {profile.Lastname}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? profile.Username : fullName;
    }

    private static string GetInitials(PublicUserProfileDTO profile)
    {
        var firstInitial = GetFirstCharacter(profile.Firstname);
        var lastInitial = GetFirstCharacter(profile.Lastname);

        if(!string.IsNullOrWhiteSpace(firstInitial + lastInitial))
            return $"{firstInitial}{lastInitial}".ToUpperInvariant();

        var username = profile.Username;
        if(string.IsNullOrWhiteSpace(username))
            return "?";

        var trimmed = username.Trim();
        if(trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return trimmed[..2].ToUpperInvariant();
    }

    private static string GetFirstCharacter(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? string.Empty : trimmed[..1];
    }
}
