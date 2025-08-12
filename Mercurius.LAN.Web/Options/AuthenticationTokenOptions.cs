namespace Mercurius.LAN.Web.Options;

public class AuthenticationTokenOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpiryHours { get; set; }
    public int RefreshTokenExpiryHours { get; set; }
}