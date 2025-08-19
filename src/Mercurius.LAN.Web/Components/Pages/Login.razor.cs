// Moved @code block from Login.razor
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Mercurius.LAN.Web.Components.Pages;

public partial class Login
{
    [CascadingParameter] private HttpContext? HttpContext { get; set; }

    [Inject] private IAuthenticationClient AuthenticationClient { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [SupplyParameterFromForm] private LoginRequest LoginRequest { get; set; } = new();

    private string _errorMessage = string.Empty;

    protected override void OnInitialized()
    {
        if(AuthService.IsLoggedIn)
        {
            NavigationManager.NavigateTo("/", true);
        }
    }

    private async Task HandleLoginAsync()
    {
        try
        {
            var loginResponse = await AuthenticationClient.LoginAsync(LoginRequest);

            if(HttpContext != null)
            {
                var accessTokenExpiry = int.Parse(Configuration["Jwt:AccessTokenExpiryHours"]);
                var refreshTokenExpiry = int.Parse(Configuration["Jwt:RefreshTokenExpiryHours"]);

                HttpContext.Response.Cookies.Append("access_token", loginResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(accessTokenExpiry)
                });

                HttpContext.Response.Cookies.Append("refresh_token", loginResponse.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(refreshTokenExpiry)
                });
            }
            await AuthService.LoginAsync(loginResponse);
            NavigationManager.NavigateTo("/", true);
        }
        catch(Exception ex)
        {
            _errorMessage = ex.Message;
        }
    }
}