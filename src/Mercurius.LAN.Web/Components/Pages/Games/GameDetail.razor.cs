using Blazored.Toast.Services;
using Mercurius.LAN.Web.Models.Games;
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
        return string.IsNullOrWhiteSpace(imageUrl)
            ? string.Empty
            : Configuration["MercuriusAPI:BaseAddress"] + imageUrl;
    }
}
