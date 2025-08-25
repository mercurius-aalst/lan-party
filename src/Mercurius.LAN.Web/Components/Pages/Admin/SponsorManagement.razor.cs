using Blazored.Toast.Services;
using Mercurius.LAN.Web.Components.Shared;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.Models.Participants;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Refit;

namespace Mercurius.LAN.Web.Components.Pages.Admin;

public partial class SponsorManagement
{
    private List<SponsorManagementDTO> _sponsors = new();
    private SponsorManagementDTO _selectedSponsor;
    private bool _isCreateMode = true;

    private CustomAutocomplete<SponsorManagementDTO> _autoCompleteComponent = null!;

    [Inject]
    private ISponsorService SponsorService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
                _sponsors = (await SponsorService.GetSponsorsAsync()).Select(sp => new SponsorManagementDTO(sp)).ToList();
                await InvokeAsync(StateHasChanged);
            }
            catch(Exception)
            {
                ToastService.ShowError("Sponsors could not be loaded.");
            }
        }
    }

    private void OnSponsorSelected(SponsorManagementDTO sponsor)
    {
        _selectedSponsor = sponsor;
        _isCreateMode = false;
    }

    private void ClearForm()
    {
        _selectedSponsor = new SponsorManagementDTO();
        _isCreateMode = true;
        _autoCompleteComponent.ClearSearchField();
        StateHasChanged();
    }

    private void UploadFile(InputFileChangeEventArgs e)
    {
        _selectedSponsor.Logo = e.File;
    }
    private async Task HandleSubmit()
    {
        if(_selectedSponsor.SponsorTier < 1)
        {
            ToastService.ShowError("Sponsor Tier must be at least 1.");
        }
        try
        {
            if(_isCreateMode)
            {
                if(_selectedSponsor.Logo is null)
                {
                    ToastService.ShowError("A sponsor logo is required.");
                    return;
                }
                var sponsor = await SponsorService.CreateSponsorAsync(new SponsorManagementDTO
                {
                    Name = _selectedSponsor!.Name,
                    InfoUrl = _selectedSponsor.InfoUrl,
                    SponsorTier = _selectedSponsor.SponsorTier,
                    Logo = _selectedSponsor.Logo
                });
                _sponsors.Add(new SponsorManagementDTO(sponsor));
                ToastService.ShowSuccess("Sponsor created successfully.");
            }
            else
            {
                await SponsorService.UpdateSponsorAsync(_selectedSponsor.Id, new SponsorManagementDTO
                {
                    Name = _selectedSponsor.Name,
                    InfoUrl = _selectedSponsor.InfoUrl,
                    SponsorTier = _selectedSponsor.SponsorTier,
                    Logo = _selectedSponsor.Logo
                });
                ToastService.ShowSuccess("Sponsor updated successfully.");
            }
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }

    private async Task DeleteSponsor()
    {
        if(_selectedSponsor == null)
            return;
        try
        {
            await SponsorService.DeleteSponsorAsync(_selectedSponsor.Id);
            _sponsors.Remove(_selectedSponsor);
            ToastService.ShowSuccess("Player deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content);
        }
    }
}