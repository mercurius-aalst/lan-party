using Blazored.Toast.Services;
using Mercurius.LAN.Web.DTOs.Games;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;
using System.Reflection;

namespace Mercurius.LAN.Web.Components.Pages.Games;

public partial class AddGameDialog
{
    [Parameter]
    public EventCallback<GameExtended> OnClose { get; set; }

    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private CreateGameDTO _newGame = new();
    private bool _isDialogOpen = true;
    private EditContext? _editContext;


    protected override void OnInitialized() {
       
        _editContext = new(_newGame);
       _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => {
            _editContext.Validate();
        };
    }
    private async Task SubmitGameAsync(EditContext editContext)
    {
            try
            {
                
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