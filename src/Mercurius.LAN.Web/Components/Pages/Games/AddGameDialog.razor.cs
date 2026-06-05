using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
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
    public EventCallback<GameExtended?> OnClose { get; set; }

    [Inject]
    private IGameService GameService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private CreateGameDTO _newGame = new();
    private bool _isDialogOpen = true;
    private EditContext? _editContext;
    private CustomInputFile? _imageInputRef;
    private DateTime? _plannedStartDate;
    private TimeSpan? _plannedStartTime;
    private string? _plannedStartTimeError;

    private static readonly BracketType[] SupportedBracketTypes =
    [
        BracketType.SingleElimination,
        BracketType.DoubleElimination
    ];


    protected override void OnInitialized() {

        SetPlannedStartInputs(_newGame.PlannedStartTime);
        _editContext = new(_newGame);
       _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => {
            _editContext.Validate();
        };
    }
    private async Task SubmitGameAsync(EditContext editContext)
    {
        string? tempFilePath = _imageInputRef?.TempFilePath;
        string? contentType = _imageInputRef?.FileContentType;
        string? fileName = _imageInputRef?.FileName;

        if(!TryApplyPlannedStartTime())
            return;

        try
            {
                
                var createdGame = await GameService.CreateGameAsync(_newGame, tempFilePath,contentType,fileName);
                ToastService.ShowSuccess($"{createdGame.Name} successfully created.");
                await OnClose.InvokeAsync(createdGame);
            }
            catch(ApiException ex)
            {
                ToastService.ShowError(ex.Content!);
            }
    }

    private bool TryApplyPlannedStartTime()
    {
        _plannedStartTimeError = null;

        if(!_plannedStartDate.HasValue)
        {
            _plannedStartTimeError = "Choose a valid planned start date.";
            return false;
        }

        if(!_plannedStartTime.HasValue)
        {
            _plannedStartTimeError = "Choose a valid planned start time.";
            return false;
        }

        var plannedStartTime = _plannedStartDate.Value
            .Date
            .Add(_plannedStartTime.Value);

        _newGame.PlannedStartTime = DateTime.SpecifyKind(plannedStartTime, DateTimeKind.Local);
        return true;
    }

    private void HandlePlannedStartDateChanged(DateTime? plannedStartDate)
    {
        _plannedStartDate = plannedStartDate;
    }

    private void HandlePlannedStartTimeChanged(TimeSpan? plannedStartTime)
    {
        _plannedStartTime = plannedStartTime;
    }

    private void SetPlannedStartInputs(DateTime plannedStartTime)
    {
        _plannedStartDate = plannedStartTime.Date;
        _plannedStartTime = plannedStartTime.TimeOfDay;
    }

    private void CloseDialog(GameExtended? createdGame)
    {
        _isDialogOpen = false;
        OnClose.InvokeAsync(createdGame);
    }
}
