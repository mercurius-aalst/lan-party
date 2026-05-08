using Auth0.AspNetCore.Authentication;
using Blazored.Toast;
using Mercurius.LAN.Web.Components;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredToast();
builder.Services.AddAntiforgery();
var jsonOptions = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() },
    AllowOutOfOrderMetadataProperties = true
};

builder.Services.AddCustomOptions(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddHttpClients(jsonOptions, builder.Configuration);
builder.Services.AddCustomServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/account/login", async (HttpContext httpContext, string? returnUrl = null) =>
{
    var redirectUri = GetSafeLocalReturnUrl(returnUrl);
    var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri(redirectUri)
            .Build();

    await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
}).AllowAnonymous();

app.MapGet("/account/logout", async (HttpContext httpContext) =>
{
    var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

    await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}).RequireAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static string GetSafeLocalReturnUrl(string? returnUrl)
{
    if(string.IsNullOrWhiteSpace(returnUrl))
        return "/";

    if(!Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        return "/";

    if(!returnUrl.StartsWith("/", StringComparison.Ordinal) ||
       returnUrl.StartsWith("//", StringComparison.Ordinal) ||
       returnUrl.StartsWith("/\\", StringComparison.Ordinal))
    {
        return "/";
    }

    return returnUrl;
}
