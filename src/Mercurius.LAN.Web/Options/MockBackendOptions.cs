namespace Mercurius.LAN.Web.Options;

public sealed class MockBackendOptions
{
    public const string SectionName = "MockBackend";

    public bool Enabled { get; init; }

    public string DataFilePath { get; init; } = "MockData.Local/backend.json";

    public string Persona { get; init; } = "user";
}
