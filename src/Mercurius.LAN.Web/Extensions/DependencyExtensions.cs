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

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))                    
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["access_token"];
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    if(!context.HttpContext.User.Identity.IsAuthenticated)
                    {
                        context.Response.Redirect("/login");
                    }
                    return Task.CompletedTask;
                }
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

        var baseAddress = configuration.GetValue<string>("MercuriusAPI:BaseAddress");

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
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<IParticipantService, ParticipantService>();

        return services;
    }
}