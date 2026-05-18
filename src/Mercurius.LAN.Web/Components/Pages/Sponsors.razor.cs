using Blazored.Toast.Services;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Models.Sponsors;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages
{
    public partial class Sponsors : ComponentBase
    {
        private List<Sponsor> _sponsors = [];
        private bool _isLoading = true;
        private static readonly IReadOnlyList<SponsorTier> TierOrder = [SponsorTier.Presenting, SponsorTier.Gold, SponsorTier.Silver, SponsorTier.Bronze];

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
                _sponsors = (await SponsorService.GetSponsorsAsync())
                    .OrderBy(sponsor => sponsor.SponsorTier.GetDisplayOrder())
                    .ThenBy(sponsor => sponsor.Name)
                    .ToList();
            }
            catch(Exception)
            {
                ToastService.ShowError("Failed to load sponsors.");
            }
            finally
            {
                _isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private IEnumerable<Sponsor> GetSponsorsByTier(SponsorTier tier)
        {
            return _sponsors
                .Where(sponsor => sponsor.SponsorTier == tier)
                .OrderBy(sponsor => sponsor.Name);
        }

        private bool HasSponsorsInTier(SponsorTier tier)
        {
            return _sponsors.Any(sponsor => sponsor.SponsorTier == tier);
        }

        private string GetTierSummary(SponsorTier tier)
        {
            var count = GetSponsorsByTier(tier).Count();
            return $"{count} partner{(count == 1 ? string.Empty : "s")} in this tier.";
        }

        private static string GetTierGridClass(SponsorTier tier)
        {
            return tier switch
            {
                SponsorTier.Presenting => "sponsor-grid sponsor-grid--presenting",
                SponsorTier.Gold => "sponsor-grid sponsor-grid--gold",
                SponsorTier.Silver => "sponsor-grid sponsor-grid--silver",
                _ => "sponsor-grid sponsor-grid--bronze"
            };
        }

        private static string GetSponsorCardClass(SponsorTier tier)
        {
            return tier switch
            {
                SponsorTier.Presenting => "brand-card sponsor-card sponsor-card--feature",
                SponsorTier.Gold => "brand-card sponsor-card sponsor-card--gold",
                SponsorTier.Silver => "brand-card sponsor-card sponsor-card--silver",
                _ => "brand-card sponsor-card sponsor-card--bronze"
            };
        }
    }
}
