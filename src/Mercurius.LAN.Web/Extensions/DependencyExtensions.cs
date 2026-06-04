using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Mock;
using Mercurius.LAN.Web.Middleware;
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Services;
using Polly;
using Refit;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Extensions;

public static class DependencyExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        if(IsMockBackendEnabled(configuration))
        {
            services.AddAuthorization();
            services.AddCascadingAuthenticationState();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/account/login";
                    options.LogoutPath = "/account/logout";
                    options.AccessDeniedPath = "/";
                    options.ClaimsIssuer = "MercuriusMock";
                    options.Cookie.Name = "mercurius-mock-auth";
                });

            return services;
        }

        var auth0Options = GetAuth0Options(configuration);

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = auth0Options.Domain;
            options.ClientId = auth0Options.ClientId;
            options.ClientSecret = auth0Options.ClientSecret;
            options.Scope = auth0Options.Scope;
            options.ResponseType = "code";
        })
        .WithAccessToken(options =>
        {
            options.Audience = auth0Options.Audience;
            options.Scope = auth0Options.Scope;
            options.UseRefreshTokens = auth0Options.UseRefreshTokens;
        });

        services.Configure<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme, options =>
        {
            options.SaveTokens = true;
            options.TokenValidationParameters.NameClaimType = "name";
            options.TokenValidationParameters.RoleClaimType = auth0Options.RoleClaimType;
            options.Events ??= new OpenIdConnectEvents();
            options.Events.OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(BuildLoginFailureRedirectUri(context.Properties?.RedirectUri, "cancelled"));
                return Task.CompletedTask;
            };
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect(BuildLoginFailureRedirectUri(context.Properties?.RedirectUri, "failed"));
                return Task.CompletedTask;
            };
        });

        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, JsonSerializerOptions jsonOptions, IConfiguration configuration)
    {
        if(IsMockBackendEnabled(configuration))
            return services;

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };

        services.AddTransient<AccessTokenHandler>();

        var configuredBaseAddress = configuration.GetValue<string>("MercuriusAPI:BaseAddress")!;
        var baseAddress = $"{configuredBaseAddress.TrimEnd('/')}/v1";

        services.AddRefitClient<ILANClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
            {
                UseCookies = false
            })
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
            }));

        services.AddRefitClient<IUserClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .ConfigurePrimaryHttpMessageHandler(static () => new HttpClientHandler
            {
                UseCookies = false
            })
            .AddHttpMessageHandler<AccessTokenHandler>();

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IContactEmailService, SmtpContactEmailService>();

        if(IsMockBackendEnabled(configuration))
        {
            services.AddSingleton<MockBackendStore>();
            services.AddScoped<IGameService, MockGameService>();
            services.AddScoped<ITeamService, MockTeamService>();
            services.AddScoped<ISponsorService, MockSponsorService>();
            services.AddScoped<IGlobalSearchService, MockGlobalSearchService>();
            services.AddScoped<IPublicProfileService, MockPublicProfileService>();
            services.AddScoped<IUserClient, MockUserClient>();
            services.AddHttpContextAccessor();
            return services;
        }

        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<ISponsorService, SponsorService>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();
        services.AddScoped<IPublicProfileService, PublicProfileService>();
        services.AddHttpContextAccessor();

        return services;
    }

    public static bool IsMockBackendEnabled(IConfiguration configuration) =>
        configuration.GetValue<bool>($"{MockBackendOptions.SectionName}:Enabled");

    public static string NormalizeMockPersona(string? persona, string fallbackPersona)
    {
        var candidate = string.IsNullOrWhiteSpace(persona) ? fallbackPersona : persona;

        return candidate.Trim().ToLowerInvariant() switch
        {
            "admin" => "admin",
            "anonymous" => "anonymous",
            _ => "user"
        };
    }

    internal static ClaimsPrincipal BuildMockPrincipal(string persona, MockBackendStore store)
    {
        var normalizedPersona = NormalizeMockPersona(persona, "user");
        var claims = new List<Claim>
        {
            new("mock_persona", normalizedPersona)
        };

        if(!string.Equals(normalizedPersona, "anonymous", StringComparison.OrdinalIgnoreCase))
        {
            var currentProfile = store.GetCurrentProfile(normalizedPersona);
            var profileUser = currentProfile.User;
            claims.Add(new Claim(ClaimTypes.Name, profileUser?.DisplayName ?? normalizedPersona));

            if(!string.IsNullOrWhiteSpace(currentProfile.Email))
                claims.Add(new Claim(ClaimTypes.Email, currentProfile.Email));

            if(profileUser != null)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, profileUser.Id.ToString()));

            if(string.Equals(normalizedPersona, "admin", StringComparison.OrdinalIgnoreCase))
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
        }

        var identity = string.Equals(normalizedPersona, "anonymous", StringComparison.OrdinalIgnoreCase)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static Auth0Options GetAuth0Options(IConfiguration configuration)
    {
        var auth0Options = configuration
            .GetRequiredSection(Auth0Options.SectionName)
            .Get<Auth0Options>() ?? new Auth0Options();

        var missingKeys = new List<string>();

        if(string.IsNullOrWhiteSpace(auth0Options.Domain))
            missingKeys.Add("Auth0:Domain");
        if(string.IsNullOrWhiteSpace(auth0Options.ClientId))
            missingKeys.Add("Auth0:ClientId");
        if(string.IsNullOrWhiteSpace(auth0Options.ClientSecret))
            missingKeys.Add("Auth0:ClientSecret");
        if(string.IsNullOrWhiteSpace(auth0Options.Audience))
            missingKeys.Add("Auth0:Audience");
        if(string.IsNullOrWhiteSpace(auth0Options.Scope))
            missingKeys.Add("Auth0:Scope");
        if(string.IsNullOrWhiteSpace(auth0Options.RoleClaimType))
            missingKeys.Add("Auth0:RoleClaimType");

        if(missingKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"Missing required Auth0 configuration: {string.Join(", ", missingKeys)}. Store Auth0:ClientSecret in user-secrets or an environment variable.");
        }

        return auth0Options;
    }

    private static string BuildLoginFailureRedirectUri(string? redirectUri, string reason)
    {
        var safeReturnUrl = GetSafeLocalReturnUrl(redirectUri);
        var uriBuilder = new UriBuilder($"http://localhost{safeReturnUrl}");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["login"] = reason;
        uriBuilder.Query = query.ToString() ?? string.Empty;

        return string.IsNullOrWhiteSpace(uriBuilder.Query)
            ? uriBuilder.Path
            : $"{uriBuilder.Path}?{uriBuilder.Query.TrimStart('?')}";
    }

    private static string GetSafeLocalReturnUrl(string? returnUrl)
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
}
