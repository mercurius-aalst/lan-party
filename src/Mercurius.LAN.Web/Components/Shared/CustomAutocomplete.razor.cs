using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class CustomAutocomplete<TItem> : IDisposable
{
    [Parameter] public List<TItem> Items { get; set; } = new();
    [Parameter] public Func<TItem, string> ItemLabel { get; set; } = default!;
    [Parameter] public EventCallback<TItem> OnSelected { get; set; }
    [Parameter] public string Placeholder { get; set; } = "Search...";

    private string _searchText = string.Empty;
    private List<TItem> _filteredItems = new();
    private bool _isDropdownVisible = false;

    private Guid _id = Guid.NewGuid();

    private ElementReference _autocompleteContainer;
    private ElementReference _dropdownElement;

    private IJSObjectReference? _outsideClickListenerModule;
    private DotNetObjectReference<CustomAutocomplete<TItem>>? _objRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isDropdownVisible && _outsideClickListenerModule == null)
        {
            _objRef = DotNetObjectReference.Create(this);
            _outsideClickListenerModule = await JSRuntime.InvokeAsync<IJSObjectReference>("addOutsideClickListener", $"dropdown-{_id}", _objRef);
        }
    }

    private void ShowDropdown()
    {
        if (_filteredItems.Any() && !string.IsNullOrEmpty(_searchText))
        {
            _isDropdownVisible = true;
            StateHasChanged();
        }
    }

    [JSInvokable]
    public void CloseDropdown()
    {
        _isDropdownVisible = false;
        StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        _filteredItems = Items;
    }

    public void ClearSearchField()
    {
        _searchText = string.Empty;
        _filteredItems = Items;
    }

    private void HandleInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(_searchText))
            _isDropdownVisible = false;
        else
        {
            _filteredItems = Items.Where(item => ItemLabel(item).Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            _isDropdownVisible = _filteredItems.Any();
        }
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && _filteredItems.Any())
        {
            var exactMatch = _filteredItems.FirstOrDefault(item => ItemLabel(item).Equals(_searchText, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
            {
                SelectItemAsync(exactMatch);
            }
        }
    }

    private async Task SelectItemAsync(TItem item)
    {
        _searchText = ItemLabel(item);
        _isDropdownVisible = false;
        await OnSelected.InvokeAsync(item);
    }

    public void Dispose()
    {
        DisposeListener();
    }

    private async void DisposeListener()
    {
        if (_outsideClickListenerModule != null)
        {
            await _outsideClickListenerModule.InvokeVoidAsync("dispose");
            await _outsideClickListenerModule.DisposeAsync();
            _outsideClickListenerModule = null;
        }
    }
}