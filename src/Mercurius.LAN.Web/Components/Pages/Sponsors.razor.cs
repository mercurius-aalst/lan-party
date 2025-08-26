using Blazored.Toast.Services;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages
{
    public partial class Sponsors : ComponentBase
    {
        private IEnumerable<Sponsor> _sponsors = [];
        [Inject]
        private ISponsorService SponsorService { get; set; } = null!;
        [Inject]
        private IToastService ToastService { get; set; } = null!;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
                await FetchSponsorsAsync();
        }

        private async Task FetchSponsorsAsync()
        {
            try
            {
                _sponsors = await SponsorService.GetSponsorsAsync();
                await InvokeAsync(StateHasChanged);
            }
            catch(Exception)
            {
                ToastService.ShowError("Failed to load sponsors.");

            }
        }
    }
}