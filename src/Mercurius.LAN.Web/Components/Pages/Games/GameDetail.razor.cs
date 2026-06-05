using Blazored.Toast.Services;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class GameDetail
{
    private enum ScheduleBracketFilter
    {
        All,
        Main,
        Lower,
        GrandFinal
    }

    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private ISponsorService SponsorService { get; set; } = null!;

    [Parameter] public Guid GameId { get; set; }

    private GameExtended? _game;
    private Match? _selectedMatch;
    private int? _selectedSponsorId;
    private List<Sponsor> _availableSponsors = [];
    private ScheduleBracketFilter _selectedScheduleBracket = ScheduleBracketFilter.All;
    private int? _selectedScheduleRound;

    private IReadOnlyList<Match> ScheduledMatches =>
        _game?.Matches
            .Where(IsScheduledMatch)
            .OrderBy(match => !match.EstimatedStartTime.HasValue ? 1 : 0)
            .ThenBy(match => match.EstimatedStartTime ?? DateTime.MaxValue)
            .ThenBy(match => match.RoundNumber)
            .ThenBy(match => match.MatchNumber)
            .ToList() ?? [];

    private IReadOnlyList<Match> FilteredScheduledMatches =>
        ScheduledMatches
            .Where(MatchesSelectedBracket)
            .Where(match => !_selectedScheduleRound.HasValue || match.RoundNumber == _selectedScheduleRound.Value)
            .ToList();

    private IReadOnlyList<int> AvailableScheduleRounds =>
        ScheduledMatches
            .Where(MatchesSelectedBracket)
            .Select(match => match.RoundNumber)
            .Distinct()
            .OrderBy(round => round)
            .ToList();

    private GameSponsorPlacement? FeaturedPartner =>
        _game?.SponsorPlacement;

    private Sponsor? SelectedSponsor =>
        _selectedSponsorId.HasValue
            ? _availableSponsors.FirstOrDefault(sponsor => sponsor.Id == _selectedSponsorId.Value)
            : null;

    private string GameSummary
    {
        get
        {
            if(_game == null)
                return string.Empty;

            var competitionType = _game.ParticipationMode == ParticipationMode.Team ? "team-based" : "solo";
            return $"Mercurius LAN {competitionType} competition with {_game.BracketType.GetLabel().ToLowerInvariant()} structure and {_game.Format.GetLabel().ToLowerInvariant()} match format.";
        }
    }

    private string ScheduleSummary
    {
        get
        {
            if(_game == null)
                return string.Empty;

            if(!ScheduledMatches.Any())
                return "No estimated matches are currently available yet.";

            var visibleMatches = FilteredScheduledMatches;
            var visibleCount = visibleMatches.Count;
            return $"{visibleCount} match{(visibleCount == 1 ? string.Empty : "es")} currently have estimated timing.";
        }
    }

    private string ScheduleCountLabel =>
        $"{FilteredScheduledMatches.Count} match{(FilteredScheduledMatches.Count == 1 ? string.Empty : "es")}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            await LoadGameDataAsync();
        }
    }

    private async Task LoadGameDataAsync()
    {
        try
        {
            var gameTask = GameService.GetGameByIdAsync(GameId);
            var sponsorsTask = _availableSponsors.Count == 0
                ? SponsorService.GetSponsorsAsync()
                : Task.FromResult<IEnumerable<Sponsor>>(_availableSponsors);

            await Task.WhenAll(gameTask, sponsorsTask);

            _game = gameTask.Result;
            _availableSponsors = sponsorsTask.Result
                .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                .ThenBy(sponsor => sponsor.Name)
                .ToList();
            SyncSelectedSponsor();
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException)
        {
            ToastService.ShowError("Could not (re)load game data");
        }
        catch(Exception)
        {
            ToastService.ShowError("Could not (re)load game data");
        }
    }

    private Task HandleGameUpdated(GameExtended updatedGame)
    {
        _game = updatedGame;
        SyncSelectedSponsor();
        return InvokeAsync(StateHasChanged);
    }

    private Task FinishGameAsync() =>
        ExecuteGameActionAsync(() => GameService.CompleteGameAsync(GameId), "Game successfully finished.");

    private Task StartGameAsync() =>
        ExecuteGameActionAsync(() => GameService.StartGameAsync(GameId), "Game successfully started.");

    private Task CancelGameAsync() =>
        ExecuteGameActionAsync(() => GameService.CancelGameAsync(GameId), "Game successfully canceled.");

    private Task ResetGameAsync() =>
        ExecuteGameActionAsync(() => GameService.ResetGameAsync(GameId), "Game successfully reset.");

    private async Task DeleteGameAsync()
    {
        try
        {
            await GameService.DeleteGameAsync(GameId);
            ToastService.ShowSuccess($"{_game?.Name} successfully deleted.");
            Navigation.NavigateTo("/games");
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private async Task ExecuteGameActionAsync(Func<Task> gameAction, string successMessage)
    {
        try
        {
            await gameAction();
            ToastService.ShowSuccess(successMessage);
            await LoadGameDataAsync();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private string GetImageUrl(string? imageUrl)
    {
        return AssetUrlResolver.Resolve(Configuration, imageUrl);
    }

    private string GetSponsorLogoUrl(string? imageUrl)
    {
        return AssetUrlResolver.Resolve(Configuration, imageUrl);
    }

    private string GetFeaturedPartnerSummary(GameSponsorPlacement placement)
    {
        return placement.SponsorDescription ?? string.Empty;
    }

    private static string GetPartnerEyebrow(GameSponsorPlacement placement)
    {
        return placement.SponsorTier == SponsorTier.Presenting
            ? "Presented by"
            : $"{placement.SponsorTier.GetShortLabel()} partner";
    }

    private string GetPageAnchorUrl(string anchorId)
    {
        return _game == null
            ? "/games"
            : $"/games/{_game.Id}#{anchorId}";
    }

    private void NavigateToRegister()
    {
        if(_game == null)
            return;

        if(!CanRegister(_game))
        {
            ToastService.ShowWarning("Registrations are closed, the tournament has already started.");
            return;
        }

        Navigation.NavigateTo(_game.RegisterFormUrl, true);
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToLocalDisplayTime().ToString("dd MMM yyyy · HH:mm");
    }

    private string GetScheduleEmptyMessage()
    {
        if(_game?.PlannedStartTime is DateTime plannedStart)
            return $"No estimated match times are available yet. Tournament planning starts {FormatDateTime(plannedStart)}.";

        return "No estimated match times are available yet.";
    }

    private string GetMatchTitle(Match match)
    {
        return $"{GetMatchParticipantName(match, true)} vs {GetMatchParticipantName(match, false)}";
    }

    private string GetMatchTimeRange(Match match)
    {
        if(!match.EstimatedStartTime.HasValue)
            return "Estimate unavailable";

        if(!match.EstimatedEndTime.HasValue || match.EstimatedEndTime <= match.EstimatedStartTime)
            return $"Start time {FormatDateTime(match.EstimatedStartTime.Value)}";

        return $"Start time {FormatDateTime(match.EstimatedStartTime.Value)} - {match.EstimatedEndTime.Value.ToLocalDisplayTime():HH:mm}";
    }

    private string GetMatchStageSummary(Match match)
    {
        var bracketLabel = GetScheduleBracketLabel(match);
        return $"{bracketLabel} · Match {match.MatchNumber}";
    }

    private string GetRoundLabel(Match match)
    {
        return $"Round {match.RoundNumber}";
    }

    private string GetScheduleStatus(Match match)
    {
        if(IsMatchDecided(match))
            return "Decided";

        return match.EstimatedStartTime.HasValue ? "Estimated" : "Awaiting estimate";
    }

    private string GetScheduleStatusClass(Match match)
    {
        if(IsMatchDecided(match))
            return "game-schedule-status--complete";

        return match.EstimatedStartTime.HasValue ? "game-schedule-status--scheduled" : "game-schedule-status--pending";
    }

    private string GetMatchParticipantName(Match match, bool firstParticipant)
    {
        if(_game == null)
            return "TBD";

        if(firstParticipant && match.Participant1IsBYE)
            return "BYE";

        if(!firstParticipant && match.Participant2IsBYE)
            return "BYE";

        return _game.ParticipationMode switch
        {
            ParticipationMode.Team => ResolveTeamName(firstParticipant ? match.TeamParticipant1Id : match.TeamParticipant2Id, _game.Teams),
            ParticipationMode.Individual => ResolveUserName(firstParticipant ? match.UserParticipant1Id : match.UserParticipant2Id, _game.Users),
            _ => "TBD"
        };
    }

    private static string ResolveTeamName(Guid? teamId, IEnumerable<Team> teams)
    {
        return teams.FirstOrDefault(team => team.Id == teamId)?.Name ?? "TBD";
    }

    private static string ResolveUserName(Guid? userId, IEnumerable<UserDTO> users)
    {
        var user = users.FirstOrDefault(candidate => candidate.Id == userId);
        if(user == null)
            return "TBD";

        return string.IsNullOrWhiteSpace(user.Username) ? user.DisplayName : user.Username;
    }

    private static bool IsMatchDecided(Match match)
    {
        return match.UserWinnerId.HasValue || match.TeamWinnerId.HasValue;
    }

    private static bool IsScheduledMatch(Match match)
    {
        return match.EstimatedStartTime.HasValue && !IsMatchDecided(match);
    }

    private static bool CanRegister(Game game)
    {
        return game.Status == GameStatus.Scheduled &&
            !string.IsNullOrWhiteSpace(game.RegisterFormUrl);
    }

    private string GetScheduleBracketLabel(Match match)
    {
        if(IsGrandFinalMatch(match))
            return "Grand final";

        return match.IsLowerBracketMatch ? "Lower bracket" : "Main bracket";
    }

    private bool IsGrandFinalMatch(Match match)
    {
        return _game?.BracketType == BracketType.DoubleElimination &&
            !match.IsLowerBracketMatch &&
            match.RoundNumber == ScheduledMatches.LastOrDefault()?.RoundNumber;
    }

    private bool MatchesSelectedBracket(Match match)
    {
        return _selectedScheduleBracket switch
        {
            ScheduleBracketFilter.Main => !match.IsLowerBracketMatch && !IsGrandFinalMatch(match),
            ScheduleBracketFilter.Lower => match.IsLowerBracketMatch,
            ScheduleBracketFilter.GrandFinal => IsGrandFinalMatch(match),
            _ => true
        };
    }

    private void HandleScheduleBracketChanged(ChangeEventArgs args)
    {
        if(Enum.TryParse<ScheduleBracketFilter>(args.Value?.ToString(), out var selectedBracket))
            _selectedScheduleBracket = selectedBracket;
        else
            _selectedScheduleBracket = ScheduleBracketFilter.All;

        if(_selectedScheduleRound.HasValue && !AvailableScheduleRounds.Contains(_selectedScheduleRound.Value))
            _selectedScheduleRound = null;
    }

    private void HandleScheduleRoundChanged(ChangeEventArgs args)
    {
        var rawValue = args.Value?.ToString();
        _selectedScheduleRound = int.TryParse(rawValue, out var parsedRound) ? parsedRound : null;
    }

    private string GetScheduleBracketFilterLabel(ScheduleBracketFilter bracketFilter) =>
        bracketFilter switch
        {
            ScheduleBracketFilter.Main => "Main bracket",
            ScheduleBracketFilter.Lower => "Lower bracket",
            ScheduleBracketFilter.GrandFinal => "Grand final",
            _ => "All brackets"
        };

    private void OpenMatchDetails(Match match)
    {
        _selectedMatch = match;
    }

    private void HandleScheduleItemKeyDown(KeyboardEventArgs args, Match match)
    {
        if(args.Key is "Enter" or " ")
        {
            OpenMatchDetails(match);
        }
    }

    private Task CloseMatchDetailsAsync()
    {
        _selectedMatch = null;
        return Task.CompletedTask;
    }

    private async Task SaveSponsorPlacementsAsync()
    {
        if(_game == null)
            return;

        try
        {
            var sponsorPlacements = _selectedSponsorId.HasValue
                ? new List<GameSponsorPlacementInputDTO>
                {
                    new()
                    {
                        SponsorId = _selectedSponsorId.Value,
                        Context = SponsorContext.TournamentPartner,
                        DisplayOrder = 1
                    }
                }
                : [];

            var updatedGame = await GameService.ReplaceGameSponsorsAsync(_game.Id, new ReplaceGameSponsorsDTO
            {
                SponsorPlacements = sponsorPlacements
            });

            _game = updatedGame;
            SyncSelectedSponsor();
            ToastService.ShowSuccess("Tournament sponsor updated.");
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }

    private void SyncSelectedSponsor()
    {
        _selectedSponsorId = _game?.SponsorPlacement?.SponsorId;
    }
}
