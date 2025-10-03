using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Refit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Middleware;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Components.Server;

namespace Mercurius.LAN.Web.Extensions;

public static class DependencyExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        services.AddAuthentication("Cookies")
            .AddCookie(options =>
            {
                options.Cookie.Name = "access_token";
                options.LoginPath = "/login";
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.Redirect("/login");
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

        services.AddRefitClient<ILANClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
            }));

        services.AddRefitClient<IAuthenticationClient>(refitSettings)
            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri(baseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>();

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
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<IParticipantService, ParticipantService>();

        return services;
    }
}