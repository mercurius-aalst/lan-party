using Mercurius.LAN.Web.Models.Games;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Games.Matches.BracketView;

public partial class SingleEliminationBracketComponent
{
    [Parameter] public GameExtended Game { get; set; } = null!;
    [Parameter] public EventCallback OnDataReload { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if(firstRender)
        {
            await JS.InvokeVoidAsync("makeDraggable", "bracket-root");
        }
    }
}
