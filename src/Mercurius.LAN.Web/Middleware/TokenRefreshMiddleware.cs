using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;
using Microsoft.AspNetCore.Http.Extensions;
using Refit;

namespace Mercurius.LAN.Web.Middleware
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthenticationClient _authClient;
        private readonly IConfiguration _configuration;

        public TokenRefreshMiddleware(RequestDelegate next, IAuthenticationClient authClient, IConfiguration configuration)
        {
            _next = next;
            _authClient = authClient;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["access_token"];
            var refreshToken = context.Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                // Attempt to refresh the token
                try
                {
                    var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
                    var tokenResponse = await _authClient.RefreshAsync(refreshRequest);

                    // Retrieve expiry durations from appsettings
                    var accessTokenExpiry = int.Parse(_configuration["Jwt:AccessTokenExpiryHours"]);
                    var refreshTokenExpiry = int.Parse(_configuration["Jwt:RefreshTokenExpiryHours"]);

                    // Set the new tokens in cookies
                    context.Response.Cookies.Append("access_token", tokenResponse.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(accessTokenExpiry)
                    });

                    context.Response.Cookies.Append("refresh_token", tokenResponse.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(refreshTokenExpiry)
                    });

                    context.Response.Redirect(context.Request.GetEncodedUrl());
                    return;
                }
                catch (ApiException ex)
                {
                    // Clear cookies and redirect to login
                    context.Response.Cookies.Delete("access_token");
                    context.Response.Cookies.Delete("refresh_token");

                    context.Response.Redirect("/login");
                    return;
                }
            }

            await _next(context);
        }
    }
}