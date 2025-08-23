using Blazored.Toast.Services;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class AddGameDialog
{
    [Parameter]
    public EventCallback<GameExtended> OnClose { get; set; }

    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private CreateGameDTO _newGame = new();
    private bool _isDialogOpen = true;
    private IBrowserFile? _uploadedFile;

    private void UploadFile(InputFileChangeEventArgs e)
    {
        _uploadedFile = e.File;
    }

    private async Task SubmitGameAsync()
    {
        try
        {
            if(_uploadedFile is null)
            {
                ToastService.ShowError("A game banner is required.");
                return;
            }
            _newGame.Image = _uploadedFile;
            var createdGame = await GameService.CreateGameAsync(_newGame);
            ToastService.ShowSuccess($"{createdGame.Name} successfully created.");
            await OnClose.InvokeAsync(createdGame);
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }

    private void CloseDialog(GameExtended createdGame)
    {
        _isDialogOpen = false;
        OnClose.InvokeAsync(createdGame);
    }
}