using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class OverviewTab
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback<GameExtended> OnGameUpdated { get; set; }

    [Inject] private IGameService GameService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private bool _isEditMode;
    private UpdateGameDTO _editGame = new();
    private EditContext? _editContext;
    private CustomInputFile? _imageInputRef;

    private void EnableEditMode()
    {
        _isEditMode = true;
        _editGame = new UpdateGameDTO
        {
            Name = Game.Name,
            Format = Game.Format,
            FinalsFormat = Game.FinalsFormat,
            BracketType = Game.BracketType,
            ParticipationMode = Game.ParticipationMode,
            RegisterFormUrl = Game.RegisterFormUrl,
            PlannedStartTime = Game.PlannedStartTime == default ? Game.StartTime : Game.PlannedStartTime,
            AverageGameDurationMinutes = Game.AverageGameDurationMinutes <= 0 ? 60 : Game.AverageGameDurationMinutes,
            RoundBreakDurationMinutes = Game.RoundBreakDurationMinutes <= 0 ? 15 : Game.RoundBreakDurationMinutes
        };
        _editContext = new(_editGame);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => _editContext.Validate();
    }

    private void CancelEditMode()
    {
        _isEditMode = false;
    }

    private string GetRegistrationStateLabel()
    {
        if(string.IsNullOrWhiteSpace(Game.RegisterFormUrl))
            return "No form linked";

        return Game.Status == GameStatus.Scheduled ? "Open via linked form" : "Closed after tournament start";
    }

    private async Task SubmitEditAsync()
    {
        string? tempFilePath = _imageInputRef?.TempFilePath;
        string? contentType = _imageInputRef?.FileContentType;
        string? fileName = _imageInputRef?.FileName;

        try
        {
            var updatedGame = await GameService.UpdateGameAsync(Game.Id, _editGame, tempFilePath, contentType, fileName);
            Game.Name = updatedGame.Name;
            Game.Format = updatedGame.Format;
            Game.FinalsFormat = updatedGame.FinalsFormat;
            Game.BracketType = updatedGame.BracketType;
            Game.ParticipationMode = updatedGame.ParticipationMode;
            Game.RegisterFormUrl = updatedGame.RegisterFormUrl;
            Game.ImageUrl = updatedGame.ImageUrl;
            Game.PlannedStartTime = updatedGame.PlannedStartTime;
            Game.AverageGameDurationMinutes = updatedGame.AverageGameDurationMinutes;
            Game.RoundBreakDurationMinutes = updatedGame.RoundBreakDurationMinutes;
            Game.EstimatedEndTime = updatedGame.EstimatedEndTime;
            _isEditMode = false;
            ToastService.ShowSuccess("Edit successful");
            await OnGameUpdated.InvokeAsync(Game);
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}
