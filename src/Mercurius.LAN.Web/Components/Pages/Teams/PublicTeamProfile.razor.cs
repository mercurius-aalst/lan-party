using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Pages.Teams;

public partial class PublicTeamProfile
{
    [Inject] private ITeamService TeamService { get; set; } = null!;

    [Parameter] public string TeamName { get; set; } = string.Empty;

    private PublicTeamProfileDTO? _team;
    private bool _isLoading;
    private bool _hasError;
    private int MemberCount => _team?.Members.Count ?? 0;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _hasError = false;
        _team = null;

        var decodedTeamName = Uri.UnescapeDataString(TeamName ?? string.Empty).Trim();
        if(string.IsNullOrWhiteSpace(decodedTeamName))
        {
            _isLoading = false;
            return;
        }

        try
        {
            _team = await TeamService.GetPublicTeamByNameAsync(decodedTeamName);
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

    private static string BuildMemberProfileHref(string username) =>
        $"/users/{Uri.EscapeDataString(username)}";

    private static string GetTeamInitials(string teamName)
    {
        if(string.IsNullOrWhiteSpace(teamName))
            return "?";

        var words = teamName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0]))
            .ToArray();

        if(words.Length == 0)
            return "?";

        return new string(words);
    }

    private static string GetMemberInitials(string username)
    {
        if(string.IsNullOrWhiteSpace(username))
            return "?";

        var trimmed = username.Trim();
        if(trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return trimmed[..2].ToUpperInvariant();
    }
}
