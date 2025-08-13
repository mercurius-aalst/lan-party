using Microsoft.Extensions.Options;
using Mercurius.LAN.Web.Options;

namespace Mercurius.LAN.Web.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AuthenticationTokenOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<MercuriusApiOptions>()
            .Bind(configuration.GetSection("MercuriusAPI"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}