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
            RegisterFormUrl = Game.RegisterFormUrl
        };
        _editContext = new(_editGame);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => _editContext.Validate();
    }

    private void CancelEditMode()
    {
        _isEditMode = false;
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
            _isEditMode = false;
            ToastService.ShowSuccess("Edit successful");
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}
