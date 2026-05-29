using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Users;

public partial class PublicUserProfile
{
    [Inject] private IPublicProfileService PublicProfileService { get; set; } = null!;

    [Parameter] public string Username { get; set; } = string.Empty;

    private PublicUserProfileDTO? _profile;
    private bool _isLoading;
    private bool _hasError;
    private bool HasLinkedIdentities =>
        !string.IsNullOrWhiteSpace(_profile?.DiscordId) ||
        !string.IsNullOrWhiteSpace(_profile?.SteamId) ||
        !string.IsNullOrWhiteSpace(_profile?.RiotId);

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _hasError = false;
        _profile = null;

        var decodedUsername = Uri.UnescapeDataString(Username ?? string.Empty).Trim();
        if(string.IsNullOrWhiteSpace(decodedUsername))
        {
            _isLoading = false;
            return;
        }

        try
        {
            _profile = await PublicProfileService.GetPublicUserByUsernameAsync(decodedUsername);
        }
        catch(Exception)
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static string GetInitials(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return "?";

        var trimmed = username.Trim();
        if(trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return trimmed[..2].ToUpperInvariant();
    }
}
