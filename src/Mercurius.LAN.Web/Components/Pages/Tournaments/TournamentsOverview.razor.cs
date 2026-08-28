using Blazored.Toast.Services;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments;

public partial class TournamentsOverview
{
    private enum TournamentSortOption
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

    private List<Tournament> _tournaments = [];
    private List<Sponsor> _sponsors = [];
    private string _searchTerm = string.Empty;
    private bool _isAddTournamentDialogOpen;
    private bool _isLoading = true;
    private bool _isLoadingPage;
    private string? _loadError;
    private int _page = 1;
    private const int PageSize = 24;
    private bool _hasNextPage;
    private TournamentSortOption _sortOption;
    private OverviewStatusFilter _statusFilter = OverviewStatusFilter.All;
    private OverviewParticipationFilter _participationFilter = OverviewParticipationFilter.All;

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private ISponsorService SponsorService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private List<Tournament> FilteredTournaments => ApplySort(ApplyFilters()).ToList();
    private int OpenRegistrationCount => FilteredTournaments.Count(CanRegister);

    private string ResultsHeading => $"{FilteredTournaments.Count} tournament{(FilteredTournaments.Count == 1 ? string.Empty : "s")}";

    private string ResultsSummary
    {
        get
        {
            if(FilteredTournaments.Count == 0)
                return "No tournaments currently match the selected filters.";

            return $"{OpenRegistrationCount} still accepting registrations across {_tournaments.Select(Tournament => Tournament.Format).Distinct().Count()} match formats.";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(!firstRender)
            return;

        await LoadPageAsync(showLoading: true);
    }

    private async Task LoadPageAsync(bool showLoading)
    {
        if(showLoading)
            _isLoading = true;
        else
            _isLoadingPage = true;

        _loadError = null;
        try
        {
            var tournamentsTask = TournamentService.GetTournamentsAsync(_page, PageSize);
            var sponsorsTask = _sponsors.Count == 0
                ? SponsorService.GetSponsorsAsync()
                : Task.FromResult<IEnumerable<Sponsor>>(_sponsors);

            await Task.WhenAll(tournamentsTask, sponsorsTask);
            _tournaments = tournamentsTask.Result;
            _hasNextPage = _tournaments.Count == PageSize;
            _sponsors = sponsorsTask.Result
                .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                .ThenBy(sponsor => sponsor.Name)
                .ToList();
        }
        catch(Exception exception)
        {
            _loadError = exception is UnauthorizedAccessException
                ? "Sign in to load the tournament list."
                : "Could not load tournaments right now.";
            ToastService.ShowError(_loadError);
        }
        finally
        {
            _isLoading = false;
            _isLoadingPage = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ChangePageAsync(int page)
    {
        if(page < 1 || page == _page || (page > _page && !_hasNextPage))
            return;

        _page = page;
        await LoadPageAsync(showLoading: false);
    }

    private Task RetryLoadAsync()
    {
        _page = 1;
        return LoadPageAsync(showLoading: true);
    }

    private void NavigateToTournamentDetail(Guid tournamentId)
    {
        NavigationManager.NavigateTo($"/tournaments/{tournamentId}");
    }

    private void ShowAddTournamentDialog()
    {
        _isAddTournamentDialogOpen = true;
    }

    private async Task CloseAddTournamentDialog(TournamentExtended? createdTournament)
    {
        _isAddTournamentDialogOpen = false;
        if(createdTournament != null)
        {
            _tournaments.Add(createdTournament);
            await InvokeAsync(StateHasChanged);
        }
    }

    private void NavigateToRegister(Tournament tournament)
    {
        if(tournament.Status != TournamentStatus.Scheduled)
        {
            ToastService.ShowWarning("Registrations are closed, the tournament has already started.");
            return;
        }

        NavigationManager.NavigateTo($"/tournaments/{tournament.Id}#tournament-participants");
    }

    private void SetStatusFilter(OverviewStatusFilter filter)
    {
        _statusFilter = filter;
    }

    private void SetParticipationFilter(OverviewParticipationFilter filter)
    {
        _participationFilter = filter;
    }

    private IEnumerable<Tournament> ApplyFilters()
    {
        return _tournaments
            .Where(MatchesSearch)
            .Where(MatchesStatusFilter)
            .Where(MatchesParticipationFilter);
    }

    private IEnumerable<Tournament> ApplySort(IEnumerable<Tournament> tournaments)
    {
        return _sortOption switch
        {
            TournamentSortOption.Name => tournaments.OrderBy(tournament => tournament.Name),
            TournamentSortOption.Status => tournaments.OrderBy(GetStatusOrder).ThenBy(GetPlannedStartForSort),
            _ => tournaments.OrderBy(GetPlannedStartForSort)
        };
    }

    private bool MatchesSearch(Tournament tournament)
    {
        return string.IsNullOrWhiteSpace(_searchTerm) ||
            tournament.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesStatusFilter(Tournament tournament)
    {
        return _statusFilter switch
        {
            OverviewStatusFilter.Open => tournament.Status == TournamentStatus.Scheduled,
            OverviewStatusFilter.Ongoing => tournament.Status == TournamentStatus.InProgress,
            OverviewStatusFilter.Finished => tournament.Status == TournamentStatus.Completed,
            OverviewStatusFilter.Cancelled => tournament.Status == TournamentStatus.Canceled,
            _ => true
        };
    }

    private bool MatchesParticipationFilter(Tournament tournament)
    {
        return _participationFilter switch
        {
            OverviewParticipationFilter.Solo => tournament.ParticipationMode == ParticipationMode.Individual,
            OverviewParticipationFilter.Team => tournament.ParticipationMode == ParticipationMode.Team,
            _ => true
        };
    }

    private static int GetStatusOrder(Tournament tournament)
    {
        return tournament.Status switch
        {
            TournamentStatus.Scheduled => 0,
            TournamentStatus.InProgress => 1,
            TournamentStatus.Completed => 2,
            TournamentStatus.Canceled => 3,
            _ => 4
        };
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToLocalDisplayTime().ToString("dd MMM · HH:mm");
    }

    private static DateTime GetPlannedStartForSort(Tournament tournament)
    {
        return tournament.PlannedStartTime;
    }

    private static string GetPlannedStartLabel(Tournament tournament)
    {
        return FormatDateTime(tournament.PlannedStartTime);
    }

    private static string GetEstimatedEndLabel(Tournament tournament)
    {
        return tournament.EstimatedEndTime.HasValue
            ? FormatDateTime(tournament.EstimatedEndTime.Value)
            : "Estimate unavailable";
    }

    private static bool CanRegister(Tournament tournament) => tournament.Status == TournamentStatus.Scheduled;
}
