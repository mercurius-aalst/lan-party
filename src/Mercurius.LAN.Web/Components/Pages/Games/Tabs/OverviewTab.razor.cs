using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;
using Blazored.Toast.Services;

namespace Mercurius.LAN.Web.Components.Pages.Games.Tabs;

public partial class OverviewTab
{
    [Parameter] public GameExtended Game { get; set; } = null!;

    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private bool _isEditMode = false;
    private UpdateGameDTO _editGame = new();
    private EditContext? _editContext;

    private void EnableEditMode()
    {
        _isEditMode = true;
        _editGame = new UpdateGameDTO
        {
            Name = Game.Name,
            Format = Enum.Parse<GameFormat>(Game.Format),
            FinalsFormat = Enum.Parse<GameFormat>(Game.FinalsFormat),
            BracketType = Game.BracketType
        };
        _editContext = new(_editGame);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => {
            _editContext.Validate();
        };
    }
    private void CancelEditMode()
    {
        _isEditMode = false;
    }

    private async Task SubmitEditAsync()
    {
        try
        {
            var updatedGame = await GameService.UpdateGameAsync(Game.Id, _editGame);
            Game.Name = updatedGame.Name;
            Game.Format = updatedGame.Format.ToString();
            Game.FinalsFormat = updatedGame.FinalsFormat.ToString();
            Game.BracketType = updatedGame.BracketType;
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