using Blazored.Toast.Services;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Home
{
    private const string HeroLocation = "Welvaartstraat 32, 9300 Aalst, Belgium";

    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private ISponsorService SponsorService { get; set; } = null!;

    private List<Game>? _games;
    private List<Sponsor> _sponsors = [];

    private IReadOnlyList<Game> FeaturedGames => _games?.Take(4).ToList() ?? [];
    private IReadOnlyList<Game> HeroGames => _games?.Take(3).ToList() ?? [];

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
            var gamesTask = GameService.GetGamesAsync();
            var sponsorsTask = SponsorService.GetSponsorsAsync();
            await Task.WhenAll(gamesTask, sponsorsTask);

            _games = gamesTask.Result;
            _sponsors = sponsorsTask.Result
                .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                .ThenBy(sponsor => sponsor.Name)
                .ToList();
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
