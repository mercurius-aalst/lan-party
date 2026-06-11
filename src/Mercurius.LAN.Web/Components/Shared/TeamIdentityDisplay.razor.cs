using Microsoft.AspNetCore.Components;

namespace Mercurius.LAN.Web.Components.Shared;

public partial class TeamIdentityDisplay
{
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? LogoUrl { get; set; }
    [Parameter] public string? Href { get; set; }
    [Parameter] public bool ShowName { get; set; } = true;
    [Parameter] public string Variant { get; set; } = "inline";
    [Parameter] public string Size { get; set; } = "medium";
    [Parameter] public string? AdditionalClass { get; set; }

    private string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Team" : Name.Trim();
    private bool HasLogo => !string.IsNullOrWhiteSpace(LogoUrl);
    private bool HasHref => !string.IsNullOrWhiteSpace(Href);
    private string Initial => string.IsNullOrWhiteSpace(DisplayName) ? "?" : DisplayName[0].ToString().ToUpperInvariant();
    private string LinkLabel => $"Open {DisplayName} team profile";

    private string ContainerClass
    {
        get
        {
            var classes = $"team-identity-display team-identity-display--{Variant} team-identity-display--{Size}";
            return string.IsNullOrWhiteSpace(AdditionalClass) ? classes : $"{classes} {AdditionalClass}";
        }
    }

    private string AvatarClass =>
        HasLogo
            ? "team-identity-avatar team-identity-avatar--image"
            : "team-identity-avatar";
}
