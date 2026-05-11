using Blazored.Toast.Services;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Home
{
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;

    private List<Game>? _games;

    private IReadOnlyList<Game> FeaturedGames => _games?.Take(4).ToList() ?? [];

    private int EventDays
    {
        get
        {
            if(_games == null || _games.Count == 0)
                return 0;

            var start = _games.Min(game => game.StartTime).Date;
            var end = _games.Max(game => game.EndTime).Date;
            return Math.Max(1, (end - start).Days + 1);
        }
    }

    private string EventWindow
    {
        get
        {
            if(_games == null || _games.Count == 0)
                return "Date To Be Announced";

            var start = _games.Min(game => game.StartTime);
            var end = _games.Max(game => game.EndTime);
            return $"{start:dd MMM yyyy} - {end:dd MMM yyyy}";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender)
            return;

        try
        {
            _games = await GameService.GetGamesAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch(Exception)
        {
            ToastService.ShowError("Could not load the home page tournaments.");
        }
    }

    private void NavigateToGame(Guid gameId)
    {
        NavigationManager.NavigateTo($"/games/{gameId}");
    }
}
