using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Options;

public sealed class Auth0Options
{
    public const string SectionName = "Auth0";

    [Required]
    public string Domain { get; init; } = string.Empty;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string Scope { get; init; } = "openid profile email";

    [Required]
    public string RoleClaimType { get; init; } = "roles";

    public bool UseRefreshTokens { get; init; }
}
