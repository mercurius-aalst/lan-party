using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Tournaments;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Tabs;

public partial class TournamentOverviewTab
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
    [Parameter] public EventCallback<TournamentExtended> OnTournamentUpdated { get; set; }

    [Inject] private ITournamentService TournamentService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private bool _isEditMode;
    private UpdateTournamentDTO _editTournament = new();
    private EditContext? _editContext;
    private CustomInputFile? _imageInputRef;
    private string? _saveError;
    private bool _isSaving;

    private static readonly BracketType[] SupportedBracketTypes =
    [
        BracketType.SingleElimination,
        BracketType.DoubleElimination
    ];

    private void EnableEditMode()
    {
        _isEditMode = true;
        _editTournament = new UpdateTournamentDTO
        {
            Name = Tournament.Name,
            Format = Tournament.Format,
            FinalsFormat = Tournament.FinalsFormat,
            BracketType = Tournament.BracketType,
            ParticipationMode = Tournament.ParticipationMode,
            TeamSize = Tournament.TeamSize,
            PlannedStartTime = Tournament.PlannedStartTime.ToLocalDisplayTime(),
            AverageGameDurationMinutes = Tournament.AverageGameDurationMinutes > 0 ? Tournament.AverageGameDurationMinutes : 30,
            RoundBreakDurationMinutes = Tournament.RoundBreakDurationMinutes > 0 ? Tournament.RoundBreakDurationMinutes : 10
        };
        _editContext = new(_editTournament);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) => _editContext.Validate();
    }

    private void CancelEditMode()
    {
        _isEditMode = false;
    }

    private string GetPlannedStartLabel() =>
        FormatDateTime(Tournament.PlannedStartTime);

    private string GetEstimatedEndLabel() =>
        Tournament.EstimatedEndTime.HasValue ? FormatDateTime(Tournament.EstimatedEndTime.Value) : "Estimate unavailable";

    private static string FormatDateTime(DateTime dateTime) =>
        dateTime.ToLocalDisplayTime().ToString("dd MMM yyyy · HH:mm");

    private string GetRegistrationStateLabel()
    {
        return Tournament.Status == TournamentStatus.Scheduled
            ? "Check eligibility on this page"
            : "Closed after tournament start";
    }

    private async Task SubmitEditAsync()
    {
        if(_isSaving)
            return;

        _saveError = null;
        _isSaving = true;
        string? tempFilePath = _imageInputRef?.TempFilePath;
        string? contentType = _imageInputRef?.FileContentType;
        string? fileName = _imageInputRef?.FileName;

        try
        {
            var updatedTournament = await TournamentService.UpdateTournamentAsync(Tournament.Id, _editTournament, tempFilePath, contentType, fileName);
            Tournament.Name = updatedTournament.Name;
            Tournament.Format = updatedTournament.Format;
            Tournament.FinalsFormat = updatedTournament.FinalsFormat;
            Tournament.BracketType = updatedTournament.BracketType;
            Tournament.ParticipationMode = updatedTournament.ParticipationMode;
            Tournament.TeamSize = updatedTournament.TeamSize;
            Tournament.PlannedStartTime = updatedTournament.PlannedStartTime;
            Tournament.AverageGameDurationMinutes = updatedTournament.AverageGameDurationMinutes;
            Tournament.RoundBreakDurationMinutes = updatedTournament.RoundBreakDurationMinutes;
            Tournament.EstimatedEndTime = updatedTournament.EstimatedEndTime;
            Tournament.ImageUrl = updatedTournament.ImageUrl;
            _isEditMode = false;
            ToastService.ShowSuccess("Edit successful");
            await OnTournamentUpdated.InvokeAsync(Tournament);
            await InvokeAsync(StateHasChanged);
        }
        catch(ApiException ex)
        {
            _saveError = string.IsNullOrWhiteSpace(ex.Content) ? "The tournament could not be updated." : ex.Content;
            ToastService.ShowError(_saveError);
        }
        catch(UnauthorizedAccessException)
        {
            _saveError = "You are not authorized to update this tournament.";
            ToastService.ShowError(_saveError);
        }
        catch(Exception)
        {
            _saveError = "The tournament could not be updated right now.";
            ToastService.ShowError(_saveError);
        }
        finally
        {
            _isSaving = false;
        }
    }
}
