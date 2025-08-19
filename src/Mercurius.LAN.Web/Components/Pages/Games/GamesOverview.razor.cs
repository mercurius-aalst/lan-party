using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class GamesOverview
{
    private List<Game> _games = new();
    private string _searchTerm = string.Empty;
    private bool _isAddGameDialogOpen = false;

    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private List<Game> FilteredGames =>
        string.IsNullOrWhiteSpace(_searchTerm)
            ? _games
            : _games.Where(game => game.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

    protected override async Task OnInitializedAsync()
    {
        _games = await GameService.GetGamesAsync();
        await InvokeAsync(StateHasChanged);
    }

    private void NavigateToGameDetail(int gameId)
    {
        NavigationManager.NavigateTo($"/games/{gameId}");
    }

    private void ShowAddGameDialog()
    {
        _isAddGameDialogOpen = true;
    }

    private async void CloseAddGameDialog(GameExtended createdGame)
    {
        _isAddGameDialogOpen = false;
        if(createdGame != null)
        {
            _games.Add(createdGame);
            await InvokeAsync(StateHasChanged);
        }
    }
}