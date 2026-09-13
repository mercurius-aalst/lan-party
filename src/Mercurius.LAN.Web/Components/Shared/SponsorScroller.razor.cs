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

    private string SponsorCountClass => Sponsors.Count switch
    {
        <= 1 => "sponsor-scroller--count-1",
        2 => "sponsor-scroller--count-2",
        3 => "sponsor-scroller--count-3",
        4 => "sponsor-scroller--count-4",
        _ => "sponsor-scroller--count-overflow"
    };
}
