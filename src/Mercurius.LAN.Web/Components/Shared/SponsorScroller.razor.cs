using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;
using Refit;

namespace Mercurius.LAN.Web.Components.Shared
{
    public partial class SponsorScroller
    {
        [Inject]
        private ISponsorService SponsorService { get; set; } = null!;
        [Inject]
        private IConfiguration Configuration { get; set; } = null!;
        private List<Sponsor> _sponsors = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {
                    _sponsors = (await SponsorService.GetSponsorsAsync()).ToList();
                    await InvokeAsync(StateHasChanged);

                }
                catch(ApiException ex)
                {
                    Console.WriteLine($"Error loading sponsors: {ex.Message}");
                }
            }
        }
    }
}