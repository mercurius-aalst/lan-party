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
    private List<Sponsor> _sponsors = new();
    private List<SponsorManagementDTO> _displaySponsors = new();
    private SponsorManagementDTO _selectedSponsor = new();
    private bool _isCreateMode = true;
    private EditContext? _editContext;

    private CustomAutocomplete<SponsorManagementDTO> _autoCompleteComponent = null!;

    [Inject]
    private ISponsorService SponsorService { get; set; } = null!;
    [Inject]
    private IToastService ToastService { get; set; } = null!;

    protected override void OnInitialized()
    {
        ReInitEditContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            try
            {
                _sponsors = (await SponsorService.GetSponsorsAsync()).ToList();
                _displaySponsors = _sponsors.Select(s => new SponsorManagementDTO
                {
                    Name = s.Name,
                    InfoUrl = s.InfoUrl,
                    SponsorTier = s.SponsorTier,
                    Id = s.Id,
                    IsCreateMode = _isCreateMode
                }).ToList();
                await InvokeAsync(StateHasChanged);
            }
            catch(Exception)
            {
                ToastService.ShowError("Sponsors could not be loaded.");
            }
        }
    }

    private void ReInitEditContext()
    {
        _editContext = new(_selectedSponsor);
        _editContext.SetFieldCssClassProvider(new BootstrapValidationFieldClassProvider());
        _editContext.OnFieldChanged += (sender, args) =>
        {
            _editContext.Validate();
        };
    }
    private void OnSponsorSelected(SponsorManagementDTO sponsor)
    {
        _selectedSponsor = sponsor;
        _isCreateMode = false;
        ReInitEditContext();
        _selectedSponsor.IsCreateMode = false;
        StateHasChanged();
    }

    private void ClearForm()
    {
        _selectedSponsor = new SponsorManagementDTO();
        _isCreateMode = true;
        _selectedSponsor.IsCreateMode = true;
        _displaySponsors = _sponsors.Select(s => new SponsorManagementDTO
        {
            Name = s.Name,
            InfoUrl = s.InfoUrl,
            SponsorTier = s.SponsorTier,
            Id = s.Id,
            IsCreateMode = _isCreateMode
        }).ToList();
        _autoCompleteComponent.ClearSearchField();       
        ReInitEditContext();
        StateHasChanged();
    }
    private async Task HandleSubmit()
    {

        try
        {
            if(_isCreateMode)
            {
                var sponsor = await SponsorService.CreateSponsorAsync(new SponsorManagementDTO
                {
                    Name = _selectedSponsor!.Name,
                    InfoUrl = _selectedSponsor.InfoUrl,
                    SponsorTier = _selectedSponsor.SponsorTier,
                    Logo = _selectedSponsor.Logo
                });
                _sponsors.Add(sponsor);
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
            ToastService.ShowError(ex.Content!);
        }
    }

    private async Task DeleteSponsor()
    {
        if(_selectedSponsor == null)
            return;
        try
        {
            await SponsorService.DeleteSponsorAsync(_selectedSponsor.Id);
            _sponsors.Remove(_sponsors.SingleOrDefault(_sponsors => _sponsors.Id == _selectedSponsor.Id)!);
            ToastService.ShowSuccess("Sponsor deleted successfully.");
            ClearForm();
        }
        catch(ApiException ex)
        {
            ToastService.ShowError(ex.Content!);
        }
    }
}