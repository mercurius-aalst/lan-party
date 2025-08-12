using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components;
using Mercurius.LAN.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Refit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.Middleware;

namespace Mercurius.LAN.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
            builder.Services.AddScoped<IGameService, GameService>();

            // Configure JsonSerializerOptions globally
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                AllowOutOfOrderMetadataProperties = true
            };

            var refitSettings = new RefitSettings
            {
                ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
            };

            builder.Services.AddRefitClient<ILANClient>(refitSettings)
                .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri($"https://localhost:7047/"))
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                }));
            builder.Services.AddRefitClient<IAuthenticationClient>(refitSettings)
                            .ConfigureHttpClient(configuration => configuration.BaseAddress = new Uri($"https://localhost:7047/"));

            // Register services
            //Authentication
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();


            //The cookie authentication is never used, but it is required to prevent a runtime error
            builder.Services.AddAuthentication(options =>
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))                    
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

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<TokenRefreshMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();


            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode();


            app.Run();
        }
    }
}
