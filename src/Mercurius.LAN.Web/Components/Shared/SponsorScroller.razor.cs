using Mercurius.LAN.Web.Models.Sponsors;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class SponsorScroller
{
    private const int LoopCount = 4;

    [Parameter, EditorRequired]
    public IReadOnlyList<Sponsor> Sponsors { get; set; } = [];

    [Parameter]
    public string AriaLabel { get; set; } = "Sponsor logos";
}
