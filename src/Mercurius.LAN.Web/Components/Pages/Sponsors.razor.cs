using Mercurius.LAN.Web.Models.Sponsors;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages
{
    public partial class Sponsors : ComponentBase
    {
        private List<Sponsor> _sponsors;

        protected override async Task OnInitializedAsync()
        {
            // Replace with actual service call to fetch sponsors
            _sponsors = await FetchSponsorsAsync();
        }

        private Task<List<Sponsor>> FetchSponsorsAsync()
        {
            // Mock data for now
            return Task.FromResult(new List<Sponsor>
            {
                new Sponsor { Name = "Sponsor A", SponsorTier = 1, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/47/PNG_transparency_demonstration_1.png", InfoUrl = "https://google.com" },
                new Sponsor { Name = "Sponsor I", SponsorTier = 1, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor K", SponsorTier = 1, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/5/51/Facebook_f_logo_%282019%29.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor B", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6a/JavaScript-logo.png", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor Crgrgefefefefefefefe", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d5/CSS3_logo_and_wordmark.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor D", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3d/Visual_Studio_Code_1.35_icon.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor M", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/4f/Csharp_Logo.png", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor N", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/8/87/Google_Chrome_icon_%282011%29.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor O", SponsorTier = 2, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/99/Unofficial_JavaScript_logo_2.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor E", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a7/React-icon.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor F", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d9/Node.js_logo.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor G", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/8/8e/Nextjs-logo.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor P", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/4/4e/Spotify_logo_with_text.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor Q", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/0/08/Netflix_2015_logo.svg", InfoUrl = "https://example.com" },
                new Sponsor { Name = "Sponsor R", SponsorTier = 3, LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/9/91/GitHub_logo.svg", InfoUrl = "https://example.com" }
            });
        }
    }
}