using Mercurius.LAN.Web.Options;

namespace Mercurius.LAN.Web.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MockBackendOptions>()
            .Bind(configuration.GetSection(MockBackendOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        if(!IsMockBackendEnabled(configuration))
        {
            services.AddOptions<Auth0Options>()
                .Bind(configuration.GetSection(Auth0Options.SectionName))
                .ValidateDataAnnotations()
                .Validate(options => !string.IsNullOrWhiteSpace(options.Domain), "Auth0:Domain is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Auth0:ClientId is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.ClientSecret), "Auth0:ClientSecret is required. Store it in user-secrets or environment variables.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Auth0:Audience is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Scope), "Auth0:Scope is required.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.RoleClaimType), "Auth0:RoleClaimType is required.")
                .ValidateOnStart();
        }

        services.AddOptions<MercuriusApiOptions>()
            .Bind(configuration.GetSection("MercuriusAPI"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<LanEventOptions>()
            .Bind(configuration.GetSection(LanEventOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => options.PackingItems.Count > 0, "LanEvent:PackingItems must contain at least one item.")
            .Validate(options => options.Tickets.Count > 0, "LanEvent:Tickets must contain at least one ticket.")
            .Validate(options => options.SocialLinks.Count > 0, "LanEvent:SocialLinks must contain at least one link.")
            .ValidateOnStart();

        services.AddOptions<ContactEmailOptions>()
            .Bind(configuration.GetSection(ContactEmailOptions.SectionName))
            .ValidateDataAnnotations();

        return services;
    }

    private static bool IsMockBackendEnabled(IConfiguration configuration)
    {
        return configuration.GetValue<bool>($"{MockBackendOptions.SectionName}:Enabled");
    }
}
