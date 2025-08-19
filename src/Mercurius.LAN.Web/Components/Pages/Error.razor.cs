// Moved @code block from Error.razor
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Error
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? _requestId;
    private bool ShowRequestId => !string.IsNullOrEmpty(_requestId);

    protected override void OnInitialized() =>
        _requestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}