using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Middleware;
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Services;
using Polly;
using Refit;
using System.Text.Json;
using System.Web;

namespace Mercurius.LAN.Web.Extensions;

public static class DependencyExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
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
        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
        };

        services.AddTransient<AccessTokenHandler>();

        var baseAddress = configuration.GetValue<string>("MercuriusAPI:BaseAddress")!;
        baseAddress = baseAddress + "/v1";

        services.AddRefitClient<ILANClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
            }));

        services.AddRefitClient<IUserClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>();

        return services;
    }

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ISponsorService, SponsorService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IParticipantService, ParticipantService>();

        return services;
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
