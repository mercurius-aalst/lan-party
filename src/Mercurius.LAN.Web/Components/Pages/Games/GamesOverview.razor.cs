using Blazored.Toast.Services;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class GamesOverview
{
    private enum GameSortOption
    {
        StartTime,
        Name,
        Status
    }

    private enum OverviewStatusFilter
    {
        All,
        Open,
        Ongoing,
        Finished,
        Cancelled
    }

    private enum OverviewParticipationFilter
    {
        All,
        Solo,
        Team
    }

    private static readonly (OverviewStatusFilter Value, string Label)[] StatusFilters =
    [
        (OverviewStatusFilter.All, "All"),
        (OverviewStatusFilter.Open, "Open"),
        (OverviewStatusFilter.Ongoing, "Ongoing"),
        (OverviewStatusFilter.Finished, "Finished"),
        (OverviewStatusFilter.Cancelled, "Cancelled")
    ];

    private static readonly (OverviewParticipationFilter Value, string Label)[] ParticipationFilters =
    [
        (OverviewParticipationFilter.All, "All"),
        (OverviewParticipationFilter.Solo, "Solo"),
        (OverviewParticipationFilter.Team, "Team")
    ];

    private List<Game> _games = [];
    private string _searchTerm = string.Empty;
    private bool _isAddGameDialogOpen;
    private bool _isLoading = true;
    private GameSortOption _sortOption;
    private OverviewStatusFilter _statusFilter = OverviewStatusFilter.All;
    private OverviewParticipationFilter _participationFilter = OverviewParticipationFilter.All;

    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private List<Game> FilteredGames => ApplySort(ApplyFilters()).ToList();

    private int OpenRegistrationCount => _games.Count(CanRegister);

    private string EventWindow
    {
        get
        {
            if(_games.Count == 0)
                return "Tournament dates will appear here once games are available.";

            var start = _games.Min(game => game.StartTime);
            var end = _games.Max(game => game.EndTime);
            return $"{start:dd MMM yyyy} - {end:dd MMM yyyy}";
        }
    }

    private string ResultsHeading => $"{FilteredGames.Count} tournament{(FilteredGames.Count == 1 ? string.Empty : "s")}";

    private string ResultsSummary
    {
        get
        {
            if(FilteredGames.Count == 0)
                return "No tournaments currently match the selected filters.";

            return $"{OpenRegistrationCount} still accepting registrations across {_games.Select(game => game.Format).Distinct().Count()} match formats.";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
                _games = await GameService.GetGamesAsync();
            }
            catch(Exception)
            {
                ToastService.ShowError("Could not load games.");
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void NavigateToGameDetail(Guid gameId)
    {
        NavigationManager.NavigateTo($"/games/{gameId}");
    }

    private void ShowAddGameDialog()
    {
        _isAddGameDialogOpen = true;
    }

    private async Task CloseAddGameDialog(GameExtended? createdGame)
    {
        _isAddGameDialogOpen = false;
        if(createdGame != null)
        {
            _games.Add(createdGame);
            await InvokeAsync(StateHasChanged);
        }
    }

    private void NavigateToRegister(Game game)
    {
        if(game.Status != GameStatus.Scheduled)
        {
            ToastService.ShowWarning("Registrations are closed, the tournament has already started.");
            return;
        }

        if(!string.IsNullOrWhiteSpace(game.RegisterFormUrl))
        {
            NavigationManager.NavigateTo(game.RegisterFormUrl, true);
        }
    }

    private void SetStatusFilter(OverviewStatusFilter filter)
    {
        _statusFilter = filter;
    }

    private void SetParticipationFilter(OverviewParticipationFilter filter)
    {
        _participationFilter = filter;
    }

    private IEnumerable<Game> ApplyFilters()
    {
        return _games
            .Where(MatchesSearch)
            .Where(MatchesStatusFilter)
            .Where(MatchesParticipationFilter);
    }

    private IEnumerable<Game> ApplySort(IEnumerable<Game> games)
    {
        return _sortOption switch
        {
            GameSortOption.Name => games.OrderBy(game => game.Name),
            GameSortOption.Status => games.OrderBy(GetStatusOrder).ThenBy(game => game.StartTime),
            _ => games.OrderBy(game => game.StartTime)
        };
    }

    private bool MatchesSearch(Game game)
    {
        return string.IsNullOrWhiteSpace(_searchTerm) ||
            game.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesStatusFilter(Game game)
    {
        return _statusFilter switch
        {
            OverviewStatusFilter.Open => game.Status == GameStatus.Scheduled,
            OverviewStatusFilter.Ongoing => game.Status is GameStatus.InProgress or GameStatus.Started,
            OverviewStatusFilter.Finished => game.Status == GameStatus.Completed,
            OverviewStatusFilter.Cancelled => game.Status == GameStatus.Canceled,
            _ => true
        };
    }

    private bool MatchesParticipationFilter(Game game)
    {
        return _participationFilter switch
        {
            OverviewParticipationFilter.Solo => game.ParticipationMode == ParticipationMode.Individual,
            OverviewParticipationFilter.Team => game.ParticipationMode == ParticipationMode.Team,
            _ => true
        };
    }

    private static int GetStatusOrder(Game game)
    {
        return game.Status switch
        {
            GameStatus.Scheduled => 0,
            GameStatus.InProgress => 1,
            GameStatus.Started => 1,
            GameStatus.Completed => 2,
            GameStatus.Canceled => 3,
            _ => 4
        };
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd MMM · HH:mm");
    }

    private static bool CanRegister(Game game)
    {
        return game.Status == GameStatus.Scheduled &&
            !string.IsNullOrWhiteSpace(game.RegisterFormUrl);
    }
}
