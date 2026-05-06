using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Polly;
using Refit;
using System.Text.Json;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Middleware;
using Mercurius.LAN.Web.Services;

namespace Mercurius.LAN.Web.Extensions;

public static class DependencyExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = configuration["Auth0:Domain"]!;
            options.ClientId = configuration["Auth0:ClientId"]!;
            options.ClientSecret = configuration["Auth0:ClientSecret"]!;
            options.Scope = configuration["Auth0:Scope"] ?? "openid profile email";
            var audience = configuration["Auth0:Audience"];
            if (!string.IsNullOrWhiteSpace(audience))
            {
                options.LoginParameters ??= new Dictionary<string, string>();
                options.LoginParameters["audience"] = audience;
            }
        });

        services.Configure<OpenIdConnectOptions>(Auth0Constants.AuthenticationScheme, options =>
        {
            options.SaveTokens = true;
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
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISponsorService, SponsorService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IParticipantService, ParticipantService>();

        return services;
    }
}
