using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;

namespace Mercurius.LAN.Web.Extensions;

internal static class LogoutState
{
    internal const string CallbackPath = "/account/logout/callback";
    internal const string CookieName = "mercurius-logout-state";
    internal const string CookiePath = "/account/logout";
    internal const string ProtectorPurpose = "Mercurius.LAN.Web.Account.LogoutState.v1";
    internal const int MaxReturnUrlLength = 1024;
    internal const int MaxProtectedStateLength = 3072;
    internal static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    public static void Store(
        HttpContext httpContext,
        IDataProtectionProvider dataProtectionProvider,
        string? returnUrl,
        bool isDevelopment)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);
        var protector = CreateProtector(dataProtectionProvider);
        var protectedState = protector.Protect(safeReturnUrl, Lifetime);

        if(protectedState.Length > MaxProtectedStateLength)
            protectedState = protector.Protect("/", Lifetime);

        httpContext.Response.Cookies.Append(
            CookieName,
            protectedState,
            CreateCookieOptions(isDevelopment, httpContext.Request.IsHttps));
    }

    public static string Consume(
        HttpContext httpContext,
        IDataProtectionProvider dataProtectionProvider,
        bool isDevelopment)
    {
        httpContext.Response.Cookies.Delete(
            CookieName,
            CreateCookieOptions(isDevelopment, httpContext.Request.IsHttps));

        if(!httpContext.Request.Cookies.TryGetValue(CookieName, out var protectedState) ||
           string.IsNullOrWhiteSpace(protectedState) ||
           protectedState.Length > MaxProtectedStateLength)
        {
            return "/";
        }

        try
        {
            var returnUrl = CreateProtector(dataProtectionProvider).Unprotect(protectedState);
            return GetSafeReturnUrl(returnUrl);
        }
        catch(CryptographicException)
        {
            return "/";
        }
    }

    internal static CookieOptions CreateCookieOptions(bool isDevelopment, bool isHttps)
    {
        return new CookieOptions
        {
            Path = CookiePath,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = !isDevelopment || isHttps,
            IsEssential = true,
            MaxAge = Lifetime,
            Expires = DateTimeOffset.UtcNow.Add(Lifetime)
        };
    }

    internal static string GetSafeReturnUrl(string? returnUrl)
    {
        var safeReturnUrl = LocalReturnUrlHelper.GetSafeLogoutReturnUrl(returnUrl);
        return safeReturnUrl.Length <= MaxReturnUrlLength ? safeReturnUrl : "/";
    }

    private static ITimeLimitedDataProtector CreateProtector(IDataProtectionProvider dataProtectionProvider) =>
        dataProtectionProvider.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector();
}
