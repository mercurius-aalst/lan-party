using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mercurius.LAN.Web.Extensions;

public static class AccountLogoutFlow
{
    public const string LogoutCallbackPath = "/account/logout/callback";

    public static async Task BeginLiveAsync(HttpContext httpContext, string? returnUrl, ILogger logger)
    {
        var redirectUri = LocalReturnUrlHelper.GetSafeLogoutReturnUrl(returnUrl);
        var callbackUri = LogoutCallbackPath + QueryString.Create("returnUrl", redirectUri).ToUriComponent();
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri(callbackUri)
            .Build();

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        try
        {
            await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        }
        catch(Exception exception) when(exception is not OperationCanceledException || !httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Remote sign-out failed after the local authentication cookie was cleared.");
            if(!httpContext.Response.HasStarted)
                httpContext.Response.Redirect(redirectUri);
        }
    }
}
