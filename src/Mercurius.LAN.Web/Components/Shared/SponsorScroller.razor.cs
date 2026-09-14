using Mercurius.LAN.Web.Models.Sponsors;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class SponsorScroller
{
    private const int LoopCount = 4;

    [Parameter, EditorRequired]
    public IReadOnlyList<Sponsor> Sponsors { get; set; } = [];

    [Parameter]
    public string AriaLabel { get; set; } = string.Empty;

    private IReadOnlyList<Sponsor>? _displaySponsors;
    private IReadOnlyList<Sponsor>? _displaySponsorsSource;
    private string _sponsorCountClass = "sponsor-scroller--count-1";

    private IReadOnlyList<Sponsor> DisplaySponsors
    {
        get
        {
            EnsureDisplaySponsors();
            return _displaySponsors!;
        }
    }

    private string SponsorCountClass
    {
        get
        {
            EnsureDisplaySponsors();
            return _sponsorCountClass;
        }
    }

    // The markup reads these properties several times per render, so the distinct list and its
    // count class are derived once per parameter instance. Assigning a new Sponsors list
    // invalidates the cache; the parameter is never mutated in place by its callers.
    private void EnsureDisplaySponsors()
    {
        if(_displaySponsors is not null && ReferenceEquals(_displaySponsorsSource, Sponsors))
            return;

        _displaySponsors = Sponsors.DistinctBy(sponsor => sponsor.Id).ToList();
        _displaySponsorsSource = Sponsors;
        _sponsorCountClass = _displaySponsors.Count switch
        {
            <= 1 => "sponsor-scroller--count-1",
            2 => "sponsor-scroller--count-2",
            3 => "sponsor-scroller--count-3",
            4 => "sponsor-scroller--count-4",
            _ => "sponsor-scroller--count-overflow"
        };
    }
}
