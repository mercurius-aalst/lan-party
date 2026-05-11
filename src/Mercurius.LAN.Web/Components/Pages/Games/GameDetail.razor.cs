using Blazored.Toast.Services;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class GameDetail
{
    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;

    [Parameter] public Guid GameId { get; set; }

    private GameExtended? _game;
    private int _selectedTab;

    private int ParticipantCount => _game == null ? 0 : GetParticipants(_game).Count();
    private int MatchCount => _game?.Matches.Count() ?? 0;
    private int RoundCount => _game?.Matches.Select(match => match.RoundNumber).Distinct().Count() ?? 0;
    private int CompletedMatchCount => _game?.Matches.Count(IsMatchDecided) ?? 0;
    private int PlacementCount => _game?.Placements.Count() ?? 0;
    private string ParticipantLabel => _game?.ParticipationMode == ParticipationMode.Team ? "teams" : "players";
    private string ParticipantLabelAbbreviation => _game?.ParticipationMode == ParticipationMode.Team ? "TEAM" : "SOLO";

    private IReadOnlyList<string> ParticipantPreviewNames =>
        _game == null
            ? []
            : GetParticipants(_game).Take(6).ToList();

    private IReadOnlyList<Match> UpcomingMatches =>
        _game?.Matches
            .OrderBy(match => match.StartTime)
            .ThenBy(match => match.RoundNumber)
            .ThenBy(match => match.MatchNumber)
            .Take(4)
            .ToList() ?? [];

    private string GameSummary
    {
        get
        {
            if(_game == null)
                return string.Empty;

            var competitionType = _game.ParticipationMode == ParticipationMode.Team ? "team-based" : "solo";
            return $"Mercurius LAN {competitionType} competition with {_game.BracketType.GetLabel().ToLowerInvariant()} rounds and {_game.Format.GetLabel().ToLowerInvariant()} matches.";
        }
    }

    private string RegistrationSummary
    {
        get
        {
            if(_game == null)
                return string.Empty;

            return CanRegister(_game)
                ? "Registration is currently available through the linked form for this tournament."
                : "Registration is closed because the tournament has moved beyond the scheduled state or no registration link is available.";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            await LoadGameDataAsync();
        }
    }

    private void SelectTab(int tab)
    {
        _selectedTab = tab;
    }

    private async Task LoadGameDataAsync()
    {
        try
        {
            _game = await GameService.GetGameByIdAsync(GameId);
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException)
        {
            ToastService.ShowError("Could not (re)load game data");
        }
    }

    private Task HandleGameUpdated(GameExtended updatedGame)
    {
        _game = updatedGame;
        return InvokeAsync(StateHasChanged);
    }

    private void OnTabDropdownChanged(ChangeEventArgs e)
    {
        if(int.TryParse(e.Value?.ToString(), out int tab))
        {
            _selectedTab = tab;
        }
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

    private string GetTabClass(int tab)
    {
        return _selectedTab == tab ? "is-active" : string.Empty;
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("dd MMM yyyy · HH:mm");
    }

    private string GetMatchTitle(Match match)
    {
        return $"{GetMatchParticipantName(match, true)} vs {GetMatchParticipantName(match, false)}";
    }

    private string GetMatchSchedule(Match match)
    {
        var startLabel = match.StartTime == default ? "Start time TBD" : FormatDateTime(match.StartTime);
        return $"Match {match.MatchNumber} · {startLabel}";
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

    private static IReadOnlyList<string> GetParticipants(GameExtended game)
    {
        return game.ParticipationMode switch
        {
            ParticipationMode.Team => game.Teams.Select(team => team.Name).ToList(),
            ParticipationMode.Individual => game.Users.Select(user => string.IsNullOrWhiteSpace(user.Username) ? user.DisplayName : user.Username!).ToList(),
            _ => []
        };
    }

    private static bool IsMatchDecided(Match match)
    {
        return match.UserWinnerId.HasValue || match.TeamWinnerId.HasValue;
    }

    private static bool CanRegister(Game game)
    {
        return game.Status == GameStatus.Scheduled &&
            !string.IsNullOrWhiteSpace(game.RegisterFormUrl);
    }
}
