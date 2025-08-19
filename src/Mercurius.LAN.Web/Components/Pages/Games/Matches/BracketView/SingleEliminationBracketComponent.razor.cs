using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class SingleEliminationBracketComponent
{
    [Parameter]
    public IEnumerable<Match> Matches { get; set; } = Enumerable.Empty<Match>();
    [Parameter]
    public IEnumerable<Participant> Participants { get; set; } = Enumerable.Empty<Participant>();
    [Parameter]
    public EventCallback OnDataReload { get; set; }

    [Inject]
    private IJSRuntime JS { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            await JS.InvokeVoidAsync("makeDraggable", "bracket-root");
        }
    }
}