using Auth0.AspNetCore.Authentication;
using Blazored.Toast;
using Mercurius.LAN.Web.Components;
using Mercurius.LAN.Web.Extensions;
using Mercurius.LAN.Web.Middleware;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazoredToast();
builder.Services.AddMudServices();
builder.Services.AddAntiforgery();
var jsonOptions = new JsonSerializerOptions
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter(), new LocalDateTimeJsonConverter() },
    AllowOutOfOrderMetadataProperties = true
};

builder.Services.AddCustomOptions(builder.Configuration);
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddHttpClients(jsonOptions, builder.Configuration);
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();
var mockModeEnabled = DependencyExtensions.IsMockBackendEnabled(app.Configuration);

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

if(mockModeEnabled)
{
    app.MapGet("/account/login", async (HttpContext httpContext, MockBackendStore store, string? returnUrl = null, string? persona = null) =>
    {
        var mockOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MockBackendOptions>>().Value;
        var resolvedPersona = DependencyExtensions.NormalizeMockPersona(persona, mockOptions.Persona);
        var principal = DependencyExtensions.BuildMockPrincipal(resolvedPersona, store);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { RedirectUri = GetSafeLocalReturnUrl(returnUrl) });

        return Results.LocalRedirect(GetSafeLocalReturnUrl(returnUrl));
    }).AllowAnonymous();

    app.MapGet("/account/register", async (HttpContext httpContext, MockBackendStore store, string? returnUrl = null, string? persona = null) =>
    {
        var mockOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<MockBackendOptions>>().Value;
        var resolvedPersona = DependencyExtensions.NormalizeMockPersona(persona, mockOptions.Persona);
        var principal = DependencyExtensions.BuildMockPrincipal(resolvedPersona, store);
        var redirectUri = BuildRegistrationRedirectUri(returnUrl);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { RedirectUri = redirectUri });

        return Results.LocalRedirect(redirectUri);
    }).AllowAnonymous();

    app.MapGet("/account/logout", async (HttpContext httpContext) =>
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.LocalRedirect("/");
    }).AllowAnonymous();
}
else
{
    app.MapGet("/account/login", async (HttpContext httpContext, string? returnUrl = null) =>
    {
        var redirectUri = GetSafeLocalReturnUrl(returnUrl);
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(redirectUri)
                .Build();

        await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
    }).AllowAnonymous();

    app.MapGet("/account/register", async (HttpContext httpContext, string? returnUrl = null) =>
    {
        var redirectUri = BuildRegistrationRedirectUri(returnUrl);
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(redirectUri)
                .WithParameter("screen_hint", "signup")
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
}

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

static string BuildRegistrationRedirectUri(string? returnUrl)
{
    var safeReturnUrl = GetSafeLocalReturnUrl(returnUrl);
    return QueryHelpers.AddQueryString(safeReturnUrl, "registration", "true");
}
