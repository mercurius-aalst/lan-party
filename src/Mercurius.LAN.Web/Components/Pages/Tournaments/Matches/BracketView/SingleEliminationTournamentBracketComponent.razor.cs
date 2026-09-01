using Mercurius.LAN.Web.Models.Tournaments;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Pages.Tournaments.Matches.BracketView;

public partial class SingleEliminationTournamentBracketComponent
{
    [Parameter] public TournamentExtended Tournament { get; set; } = null!;
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
