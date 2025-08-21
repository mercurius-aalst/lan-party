using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages
{
    public partial class Sponsors : ComponentBase
    {
        private IEnumerable<Sponsor> _sponsors;
        [Inject]
        private ISponsorService SponsorService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
           await FetchSponsorsAsync();
        }

        private async Task FetchSponsorsAsync()
        {
            _sponsors = await SponsorService.GetSponsorsAsync();
            
        }
    }
}