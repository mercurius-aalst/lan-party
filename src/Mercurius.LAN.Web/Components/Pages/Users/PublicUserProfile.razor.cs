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
    private string FullName => GetFullName(_profile);
    private string PageTitleText => _profile is null ? "User Profile" : $"{FullName} Profile";

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

    private static string GetFullName(PublicUserProfileDTO? profile)
    {
        if(profile is null)
            return string.Empty;

        var fullName = $"{profile.Firstname} {profile.Lastname}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? profile.Username : fullName;
    }

    private static string GetInitials(PublicUserProfileDTO profile)
    {
        var firstInitial = GetFirstCharacter(profile.Firstname);
        var lastInitial = GetFirstCharacter(profile.Lastname);

        if(!string.IsNullOrWhiteSpace(firstInitial + lastInitial))
            return $"{firstInitial}{lastInitial}".ToUpperInvariant();

        var username = profile.Username;
        if(string.IsNullOrWhiteSpace(username))
            return "?";

        var trimmed = username.Trim();
        if(trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return trimmed[..2].ToUpperInvariant();
    }

    private static string GetFirstCharacter(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 ? string.Empty : trimmed[..1];
    }
}
