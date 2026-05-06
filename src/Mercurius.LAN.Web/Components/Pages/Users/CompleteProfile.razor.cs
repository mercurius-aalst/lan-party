using Blazored.Toast.Services;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Users;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class CompleteProfile
{
    private readonly CompleteUserProfileRequest _model = new();

    [Inject] private IUserClient UserClient { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private async Task SaveAsync()
    {
        await UserClient.CompleteCurrentUserProfileAsync(_model);
        ToastService.ShowSuccess("Profile completed.");
        NavigationManager.NavigateTo("/", true);
    }
}
