namespace Mercurius.LAN.Web.Options;

public sealed class MockBackendOptions
{
    public const string SectionName = "MockBackend";

    public bool Enabled { get; init; }

    public string DataFilePath { get; init; } = "MockData.Local/backend.json";

    public string Persona { get; init; } = "user";
}

public static class MockBackendMode
{
    public static bool IsAllowedEnvironment(IHostEnvironment environment) =>
        environment.IsDevelopment()
        || environment.IsEnvironment("Test")
        || environment.IsEnvironment("Testing");

    public static bool Resolve(
        IConfiguration configuration,
        IHostEnvironment environment,
        bool mockBackendIncluded)
    {
        var enabled = configuration.GetValue<bool>($"{MockBackendOptions.SectionName}:Enabled");
        if(!enabled)
            return false;

        if(!mockBackendIncluded)
        {
            throw new InvalidOperationException(
                "Mock backend mode is enabled, but this build excludes the mock backend. " +
                "Disable MockBackend:Enabled or use an explicit local development/test build with IncludeMockBackend=true.");
        }

        if(IsAllowedEnvironment(environment))
            return true;

        throw new InvalidOperationException(
            $"Mock backend mode cannot be enabled in the '{environment.EnvironmentName}' environment. " +
            "Mock mode is restricted to Development, Test, and Testing environments.");
    }
}
