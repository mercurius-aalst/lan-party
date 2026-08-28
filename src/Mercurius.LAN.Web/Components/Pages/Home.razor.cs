using Blazored.Toast.Services;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Home
{
    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private ISponsorService SponsorService { get; set; } = null!;
    [Inject] private IOptions<LanEventOptions> EventOptions { get; set; } = null!;

    private List<Tournament>? _tournaments;
    private List<Sponsor> _sponsors = [];
    private string? _loadError;

    private IReadOnlyList<Tournament> FeaturedTournaments => _tournaments?.Take(4).ToList() ?? [];
    private IReadOnlyList<Tournament> HeroTournaments => _tournaments?.Take(3).ToList() ?? [];

    private string EventWindow => EventOptions.Value.EventWindow;

    private string HeroLocation => $"{EventOptions.Value.VenueName}, {EventOptions.Value.Address}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender)
            return;

        await LoadHomeDataAsync();
    }

    private async Task LoadHomeDataAsync()
    {
        _loadError = null;
        try
        {
            var tournamentsTask = TournamentService.GetTournamentsAsync(pageSize: 12);
            var sponsorsTask = SponsorService.GetSponsorsAsync();
            await Task.WhenAll(tournamentsTask, sponsorsTask);

            _tournaments = tournamentsTask.Result;
            _sponsors = sponsorsTask.Result
                .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                .ThenBy(sponsor => sponsor.Name)
                .ToList();
            await InvokeAsync(StateHasChanged);
        }
        catch(Exception)
        {
            _loadError = "Could not load the tournament highlights right now.";
            ToastService.ShowError("Could not load the home page tournaments.");
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task RetryLoadAsync() => LoadHomeDataAsync();

    private void NavigateToTournament(Guid tournamentId)
    {
        NavigationManager.NavigateTo($"/tournaments/{tournamentId}");
    }
}
