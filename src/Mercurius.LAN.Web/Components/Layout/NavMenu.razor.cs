using Mercurius.LAN.Web.DTOs.Search;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Components.Layout;

public partial class NavMenu : IAsyncDisposable
{
    private const int SearchDebounceMilliseconds = 300;
    private const int MinimumSearchQueryLength = 3;
    private const string SearchContainerElementId = "global-nav-search-container";
    private const string AccountMenuContainerElementId = "account-nav-menu-container";

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;
    [Inject]
    private IGlobalSearchService GlobalSearchService { get; set; } = null!;
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject]
    private IUserClient UserClient { get; set; } = null!;
    [Inject]
    private ITeamNotificationService NotificationService { get; set; } = null!;
    [Inject]
    private ITeamRealtimeService TeamRealtimeService { get; set; } = null!;
    [Inject]
    private ITeamService TeamService { get; set; } = null!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool _isUserMenuVisible = false;
    private bool _isDropdownVisible = false;
    private bool _isInfoMenuVisible = false;
    private bool _isNotificationMenuVisible = false;
    private string _searchQuery = string.Empty;
    private List<GlobalSearchResultDTO> _searchResults = [];
    private bool _isSearchLoading;
    private bool _hasSearchError;
    private bool _isSearchDropdownVisible;
    private int _highlightedSearchIndex = -1;
    private string? _searchNextCursor;
    private bool _searchHasMore;
    private long _searchRequestVersion;
    private CancellationTokenSource? _searchCancellationTokenSource;
    private IJSObjectReference? _searchOutsideClickListener;
    private IJSObjectReference? _accountMenuOutsideClickListener;
    private DotNetObjectReference<NavMenu>? _searchOutsideClickReference;
    private string? _loadedIdentityKey;
    private string? _currentProfileUsername;

    [Parameter]
    public EventCallback OnNavigationSelected { get; set; }

    private string LoginHref => $"/account/login?returnUrl={Uri.EscapeDataString(GetCurrentRelativeUrl())}";
    private string MockAdminLoginHref => $"/account/login?persona=admin&returnUrl={Uri.EscapeDataString("/admin/sponsors")}";
    private bool IsMockBackendEnabled => Configuration.GetValue<bool>("MockBackend:Enabled");
    private bool ShouldShowInteractionOverlay => _isUserMenuVisible || _isDropdownVisible || _isInfoMenuVisible || _isNotificationMenuVisible;
    private bool HasSearchResults => _searchResults.Count > 0;
    private int NotificationCount => NotificationService.UnreadCount;

    protected override void OnInitialized()
    {
        NotificationService.Changed += HandleNotificationsChangedAsync;
        TeamRealtimeService.TeamStateInvalidated += RefreshNotificationsFromSignalAsync;
    }

    protected override async Task OnParametersSetAsync()
    {
        if(AuthenticationStateTask == null)
            return;

        var authState = await AuthenticationStateTask;
        var user = authState.User;
        if(user.Identity?.IsAuthenticated != true)
        {
            _loadedIdentityKey = null;
            _currentProfileUsername = null;
            return;
        }

        var identityKey = GetIdentityKey(user);
        if(string.Equals(identityKey, _loadedIdentityKey, StringComparison.Ordinal))
            return;

        _loadedIdentityKey = identityKey;
        _currentProfileUsername = null;

        try
        {
            var profile = await UserClient.GetCurrentUserProfileAsync();
            _currentProfileUsername = profile.User?.Username?.Trim();
            await NotificationService.RefreshAsync();
            await TeamRealtimeService.StartAsync();
        }
        catch(Exception)
        {
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(_isSearchDropdownVisible && _searchOutsideClickListener == null)
        {
            _searchOutsideClickReference ??= DotNetObjectReference.Create(this);
            _searchOutsideClickListener = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "addNavSearchOutsideClickListener",
                SearchContainerElementId,
                _searchOutsideClickReference);
        }
        else if(!_isSearchDropdownVisible && _searchOutsideClickListener != null)
        {
            await DisposeSearchOutsideClickListenerAsync();
        }

        if((_isUserMenuVisible || _isNotificationMenuVisible) && _accountMenuOutsideClickListener == null)
        {
            _searchOutsideClickReference ??= DotNetObjectReference.Create(this);
            _accountMenuOutsideClickListener = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "addNavMenuOutsideClickListener",
                AccountMenuContainerElementId,
                _searchOutsideClickReference);
        }
        else if(!_isUserMenuVisible && !_isNotificationMenuVisible && _accountMenuOutsideClickListener != null)
        {
            await DisposeAccountMenuOutsideClickListenerAsync();
        }
    }

    private async Task BeginLogin()
    {
        CloseAllTemporarySurfaces(clearSearchResults: true, clearSearchQuery: true);
        await OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo(LoginHref, forceLoad: true);
    }

    private async Task Logout()
    {
        CloseAllTemporarySurfaces(clearSearchResults: true, clearSearchQuery: true);
        await OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo("/account/logout", true);
    }

    private void ToggleUserMenu()
    {
        var shouldOpen = !_isUserMenuVisible;
        CloseAllTemporarySurfaces();
        _isUserMenuVisible = shouldOpen;
    }

    private void ToggleNotificationMenu()
    {
        var shouldOpen = !_isNotificationMenuVisible;
        CloseAllTemporarySurfaces();
        _isNotificationMenuVisible = shouldOpen;
    }

    private void HandleOutsideClick()
    {
        CloseAllTemporarySurfaces();
    }

    private void HandleNavShellClick()
    {
        if(_isSearchDropdownVisible)
            CloseSearchDropdown(clearResults: false, clearQuery: false);
    }

    private void ToggleDropdown()
    {
        var shouldOpen = !_isDropdownVisible;
        CloseAllTemporarySurfaces();
        _isDropdownVisible = shouldOpen;
    }

    private void ToggleInfoMenu()
    {
        var shouldOpen = !_isInfoMenuVisible;
        CloseAllTemporarySurfaces();
        _isInfoMenuVisible = shouldOpen;
    }

    private void CloseAllTemporarySurfaces(bool clearSearchResults = false, bool clearSearchQuery = false)
    {
        _isDropdownVisible = false;
        _isUserMenuVisible = false;
        _isInfoMenuVisible = false;
        _isNotificationMenuVisible = false;
        CloseSearchDropdown(clearSearchResults, clearSearchQuery);
    }

    private async Task HandleNavigationClicked()
    {
        CloseAllTemporarySurfaces(clearSearchResults: true, clearSearchQuery: true);
        await OnNavigationSelected.InvokeAsync();
    }

    private async Task HandleSearchInputAsync(ChangeEventArgs args)
    {
        _searchQuery = args.Value?.ToString() ?? string.Empty;
        _highlightedSearchIndex = -1;
        _hasSearchError = false;

        var trimmedQuery = _searchQuery.Trim();
        CancelPendingSearch();

        if(trimmedQuery.Length < MinimumSearchQueryLength)
        {
            CloseSearchDropdown(clearResults: true, clearQuery: false);
            return;
        }

        _isDropdownVisible = false;
        _isUserMenuVisible = false;
        _isInfoMenuVisible = false;
        _isNotificationMenuVisible = false;
        _isSearchLoading = true;
        _isSearchDropdownVisible = true;
        _searchResults = [];

        var requestVersion = ++_searchRequestVersion;
        _searchCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _searchCancellationTokenSource.Token;

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, cancellationToken);
            var response = await GlobalSearchService.SearchAsync(trimmedQuery, cancellationToken);

            if(cancellationToken.IsCancellationRequested || requestVersion != _searchRequestVersion)
                return;

            _searchResults = response.Results.ToList();
            _searchNextCursor = response.NextCursor;
            _searchHasMore = response.HasMore;
            _isSearchLoading = false;
            _hasSearchError = false;
            _isSearchDropdownVisible = true;
        }
        catch(OperationCanceledException)
        {
        }
        catch(Exception)
        {
            if(cancellationToken.IsCancellationRequested || requestVersion != _searchRequestVersion)
                return;

            _searchResults = [];
            _isSearchLoading = false;
            _hasSearchError = true;
            _isSearchDropdownVisible = true;
        }
    }

    private void HandleSearchFocus()
    {
        if(_searchQuery.Trim().Length < MinimumSearchQueryLength)
            return;

        _isDropdownVisible = false;
        _isUserMenuVisible = false;
        _isInfoMenuVisible = false;
        _isNotificationMenuVisible = false;
        _isSearchDropdownVisible = true;
    }

    private async Task HandleSearchKeyDownAsync(KeyboardEventArgs args)
    {
        if(args.Key == "Escape")
        {
            CloseSearchDropdown(clearResults: false, clearQuery: false);
            return;
        }

        if(!_isSearchDropdownVisible || _isSearchLoading || _hasSearchError || _searchResults.Count == 0)
            return;

        if(args.Key == "ArrowDown")
        {
            _highlightedSearchIndex = _highlightedSearchIndex < _searchResults.Count - 1
                ? _highlightedSearchIndex + 1
                : 0;
            return;
        }

        if(args.Key == "ArrowUp")
        {
            _highlightedSearchIndex = _highlightedSearchIndex > 0
                ? _highlightedSearchIndex - 1
                : _searchResults.Count - 1;
            return;
        }

        if(args.Key == "Enter" &&
           _highlightedSearchIndex >= 0 &&
           _highlightedSearchIndex < _searchResults.Count)
        {
            await SelectSearchResultAsync(_searchResults[_highlightedSearchIndex]);
        }
    }

    private async Task SelectSearchResultAsync(GlobalSearchResultDTO result)
    {
        var destination = BuildSearchDestination(result);
        if(string.IsNullOrWhiteSpace(destination))
            return;

        CloseAllTemporarySurfaces(clearSearchResults: true, clearSearchQuery: true);
        await OnNavigationSelected.InvokeAsync();
        NavigationManager.NavigateTo(destination);
    }

    private Task ClearSearchInputAsync()
    {
        _searchQuery = string.Empty;
        CloseSearchDropdown(clearResults: true, clearQuery: false);
        return Task.CompletedTask;
    }

    private void SetSearchHighlight(int index)
    {
        _highlightedSearchIndex = index;
    }

    [JSInvokable]
    public async Task CloseDropdown()
    {
        if(!_isSearchDropdownVisible)
            return;

        await InvokeAsync(() =>
        {
            CloseSearchDropdown(clearResults: false, clearQuery: false);
            StateHasChanged();
        });
    }

    [JSInvokable]
    public async Task CloseAccountDropdowns()
    {
        if(!_isUserMenuVisible && !_isNotificationMenuVisible)
            return;

        await InvokeAsync(() =>
        {
            _isUserMenuVisible = false;
            _isNotificationMenuVisible = false;
            StateHasChanged();
        });
    }

    private void CloseSearchDropdown(bool clearResults, bool clearQuery)
    {
        CancelPendingSearch();
        _isSearchDropdownVisible = false;
        _isSearchLoading = false;
        _hasSearchError = false;
        _highlightedSearchIndex = -1;
        _searchNextCursor = null;
        _searchHasMore = false;

        if(clearResults)
            _searchResults = [];

        if(clearQuery)
            _searchQuery = string.Empty;
    }

    private void CancelPendingSearch()
    {
        _searchRequestVersion++;
        if(_searchCancellationTokenSource == null)
            return;

        _searchCancellationTokenSource.Cancel();
        _searchCancellationTokenSource.Dispose();
        _searchCancellationTokenSource = null;
    }

    private string GetAdminButtonClass() => GetUtilityButtonClass(_isDropdownVisible);

    private string GetInfoButtonClass()
    {
        var classes = "brand-nav-link info-nav-button text-center md:text-left";
        return _isInfoMenuVisible || IsInfoPage ? $"{classes} brand-nav-link--active" : classes;
    }

    private string GetUserButtonClass() => $"{GetUtilityButtonClass(_isUserMenuVisible)} user-button";

    private string GetNotificationButtonClass()
    {
        var classes = "brand-utility-button nav-widget-notification-button";
        return _isNotificationMenuVisible ? $"{classes} brand-utility-button--open" : classes;
    }

    private string GetSearchContainerClass()
    {
        var classes = "brand-nav-search";
        return _isSearchDropdownVisible ? $"{classes} brand-nav-search--open" : classes;
    }

    private string GetSearchInputShellClass()
    {
        var classes = "nav-search-input-shell";
        return _isSearchDropdownVisible ? $"{classes} nav-search-input-shell--open" : classes;
    }

    private bool IsInfoPage => NavigationManager.ToBaseRelativePath(NavigationManager.Uri).StartsWith("info", StringComparison.OrdinalIgnoreCase);

    private static string GetAriaExpanded(bool isExpanded) => isExpanded ? "true" : "false";

    private static string GetUtilityButtonClass(bool isOpen)
    {
        var classes = "brand-utility-button w-full justify-center md:w-auto";
        return isOpen ? $"{classes} brand-utility-button--open" : classes;
    }

    private string GetCurrentRelativeUrl()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

        return string.IsNullOrWhiteSpace(relativePath)
            ? "/"
            : $"/{relativePath}";
    }

    private static string GetNotificationItemClass(TeamNotificationItem notification)
    {
        return notification.IsRead
            ? "nav-notification-item"
            : "nav-notification-item nav-notification-item--unread";
    }

    private static string? BuildSearchDestination(GlobalSearchResultDTO result)
    {
        return result.Type switch
        {
            GlobalSearchResultType.User when !string.IsNullOrWhiteSpace(result.Username) =>
                $"/users/{Uri.EscapeDataString(result.Username)}",
            GlobalSearchResultType.Team when !string.IsNullOrWhiteSpace(result.TeamName) =>
                $"/teams/{Uri.EscapeDataString(result.TeamName)}",
            GlobalSearchResultType.Tournament when result.TournamentId.HasValue =>
                $"/tournaments/{result.TournamentId.Value}",
            _ => null
        };
    }

    private string GetDisplayName(ClaimsPrincipal user)
    {
        return _currentProfileUsername
            ?? GetUsernameClaim(user)
            ?? user.Identity?.Name
            ?? user.FindFirst("name")?.Value
            ?? user.FindFirst("email")?.Value
            ?? "Account";
    }

    private string GetUserMenuAriaLabel(ClaimsPrincipal user)
    {
        var displayName = GetDisplayName(user);

        return $"{displayName} account menu";
    }

    private string GetNotificationAriaLabel()
    {
        return NotificationCount == 0
            ? "Team notifications"
            : $"Team notifications with {NotificationCount} unread";
    }

    private static string GetIdentityKey(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value
            ?? user.Identity?.Name
            ?? "authenticated";
    }

    private static string? GetUsernameClaim(ClaimsPrincipal user)
    {
        return user.FindFirst("preferred_username")?.Value?.Trim()
            ?? user.FindFirst("nickname")?.Value?.Trim()
            ?? user.FindFirst("username")?.Value?.Trim();
    }

    private async ValueTask DisposeSearchOutsideClickListenerAsync()
    {
        var listener = _searchOutsideClickListener;
        _searchOutsideClickListener = null;

        if(listener == null)
            return;

        try
        {
            await listener.InvokeVoidAsync("dispose");
            await listener.DisposeAsync();
        }
        catch(JSDisconnectedException)
        {
        }
    }

    private async ValueTask DisposeAccountMenuOutsideClickListenerAsync()
    {
        var listener = _accountMenuOutsideClickListener;
        _accountMenuOutsideClickListener = null;

        if(listener == null)
            return;

        try
        {
            await listener.InvokeVoidAsync("dispose");
            await listener.DisposeAsync();
        }
        catch(JSDisconnectedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        NotificationService.Changed -= HandleNotificationsChangedAsync;
        TeamRealtimeService.TeamStateInvalidated -= RefreshNotificationsFromSignalAsync;
        CancelPendingSearch();
        await DisposeSearchOutsideClickListenerAsync();
        await DisposeAccountMenuOutsideClickListenerAsync();
        _searchOutsideClickReference?.Dispose();
    }

    private async Task MarkNotificationsReadAsync()
    {
        await NotificationService.MarkAllReadAsync();
    }

    private async Task DismissNotificationAsync(string id)
    {
        await NotificationService.DismissAsync(id);
    }

    private async Task RespondToInviteNotificationAsync(Guid inviteId, bool accept)
    {
        try
        {
            await TeamService.RespondToInviteAsync(inviteId, accept);
            await NotificationService.RefreshAsync();
        }
        catch(Exception)
        {
        }
    }

    private Task HandleNotificationsChangedAsync()
    {
        return InvokeAsync(StateHasChanged);
    }

    private async Task RefreshNotificationsFromSignalAsync()
    {
        try
        {
            await NotificationService.RefreshAsync();
        }
        catch(Exception)
        {
        }
    }
}
