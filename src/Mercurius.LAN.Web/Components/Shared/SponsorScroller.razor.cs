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

        private List<Sponsor> _sponsors = new();
        private const int _repeater = 5;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                try
                {
                    var sponsors = (await SponsorService.GetSponsorsAsync()).ToList();
                    for(int i = 1; i <= _repeater; i++)
                        _sponsors.AddRange(sponsors);
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