using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments;

public partial class AddTournamentDialog
{
    [Parameter]
    public EventCallback<TournamentExtended?> OnClose { get; set; }

    [Inject]
    private ITournamentService TournamentService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;
    [Inject]
    private IConfiguration Configuration { get; set; } = null!;

    private CreateTournamentDTO _newTournament = new();
    private bool _isDialogOpen = true;
    private EditContext? _editContext;
    private CustomInputFile? _imageInputRef;
    private DateTime? _plannedStartDate;
    private TimeSpan? _plannedStartTime;
    private string? _plannedStartTimeError;
    private string? _submitError;
    private bool _isSubmitting;

    private static readonly BracketType[] SupportedBracketTypes =
    [
        BracketType.SingleElimination,
        BracketType.DoubleElimination
    ];


    protected override void OnInitialized() {

        SetPlannedStartInputs(_newTournament.PlannedStartTime);
        _editContext = new(_newTournament);
       _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => {
            _editContext.Validate();
        };
    }
    private async Task SubmitTournamentAsync(EditContext editContext)
    {
        if(_isSubmitting)
            return;

        _submitError = null;
        string? tempFilePath = _imageInputRef?.TempFilePath;
        string? contentType = _imageInputRef?.FileContentType;
        string? fileName = _imageInputRef?.FileName;

        if(!TryApplyPlannedStartTime())
            return;

        _isSubmitting = true;
        try
        {
            var createdTournament = await TournamentService.CreateTournamentAsync(_newTournament, tempFilePath, contentType, fileName);
            ToastService.ShowSuccess($"{createdTournament.Name} successfully created.");
            await OnClose.InvokeAsync(createdTournament);
        }
        catch(ApiException ex)
        {
            _submitError = string.IsNullOrWhiteSpace(ex.Content)
                ? "The tournament could not be created."
                : ex.Content;
            ToastService.ShowError(_submitError);
        }
        catch(UnauthorizedAccessException)
        {
            _submitError = "You are not authorized to create tournaments.";
            ToastService.ShowError(_submitError);
        }
        catch(Exception)
        {
            _submitError = "The tournament could not be created right now.";
            ToastService.ShowError(_submitError);
        }
        finally
        {
            _isSubmitting = false;
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

        _newTournament.PlannedStartTime = DateTime.SpecifyKind(plannedStartTime, DateTimeKind.Local);
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

    private void CloseDialog(TournamentExtended? createdTournament)
    {
        _isDialogOpen = false;
        OnClose.InvokeAsync(createdTournament);
    }
}
