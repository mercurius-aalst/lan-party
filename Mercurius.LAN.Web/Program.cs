using Mercurius.LAN.Web;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Components;
using Mercurius.LAN.Web.Services;
using Polly;
using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mercurius.LAN.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();
            builder.Services.AddScoped<IGameService, GameService>();

            // Configure JsonSerializerOptions globally
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
