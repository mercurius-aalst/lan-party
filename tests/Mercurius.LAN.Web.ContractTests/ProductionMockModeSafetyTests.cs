using Mercurius.LAN.Web.Extensions;
#if INCLUDE_MOCK_BACKEND
using Mercurius.LAN.Web.Mock;
#endif
using Mercurius.LAN.Web.Options;
using Mercurius.LAN.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class ProductionMockModeSafetyTests
{
    [Fact]
    public void Resolve_RejectsEnabledMockModeInProduction()
    {
        var configuration = BuildConfiguration(enabled: true);
        var environment = new TestHostEnvironment(Environments.Production);

        var exception = Assert.Throws<InvalidOperationException>(
            () => MockBackendMode.Resolve(configuration, environment, mockBackendIncluded: true));

        Assert.Contains(Environments.Production, exception.Message, StringComparison.Ordinal);
        Assert.Contains("cannot be enabled", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Test")]
    [InlineData("Testing")]
    public void Resolve_AllowsExplicitMockModeInApprovedEnvironments(string environmentName)
    {
        var configuration = BuildConfiguration(enabled: true);
        var environment = new TestHostEnvironment(environmentName);

        Assert.True(MockBackendMode.Resolve(configuration, environment, mockBackendIncluded: true));
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void Resolve_KeepsMockModeDisabledWhenFlagIsOff(string environmentName)
    {
        var configuration = BuildConfiguration(enabled: false);
        var environment = new TestHostEnvironment(environmentName);

        Assert.False(MockBackendMode.Resolve(configuration, environment, mockBackendIncluded: true));
    }

    [Fact]
    public void Resolve_RejectsEnabledMockModeWhenBuildExcludesIt()
    {
        var configuration = BuildConfiguration(enabled: true);
        var environment = new TestHostEnvironment(Environments.Development);

        var exception = Assert.Throws<InvalidOperationException>(
            () => MockBackendMode.Resolve(configuration, environment, mockBackendIncluded: false));

        Assert.Contains("build excludes", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

#if INCLUDE_MOCK_BACKEND
    [Fact]
    public void ServiceRegistration_UsesTheValidatedRuntimeDecision()
    {
        var configuration = BuildConfiguration(enabled: true);
        var mockModeEnabled = MockBackendMode.Resolve(
            configuration,
            new TestHostEnvironment(Environments.Development),
            mockBackendIncluded: true);
        var services = new ServiceCollection();

        services.AddCustomServices(configuration, mockModeEnabled);

        var tournamentService = Assert.Single(
            services.Where(descriptor => descriptor.ServiceType == typeof(ITournamentService)));
        Assert.Equal(typeof(MockTournamentService), tournamentService.ImplementationType);
    }
#endif

    [Fact]
    public void ProjectExcludesLocalMockConfigurationAndFixturesFromPublishOutput()
    {
        var project = XDocument.Load(RepositoryPath("src", "Mercurius.LAN.Web", "Mercurius.LAN.Web.csproj"));
        var nonMockItemGroup = Assert.Single(project
            .Descendants("ItemGroup")
            .Where(element => ((string?)element.Attribute("Condition"))?.Contains(
                "IncludeMockBackend",
                StringComparison.Ordinal) == true));
        var removedItems = nonMockItemGroup
            .Elements()
            .Select(element => (string?)element.Attribute("Remove"))
            .Where(value => value is not null)
            .ToArray();

        Assert.Contains(removedItems, value => value!.Contains("Mock\\**\\*.cs", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(removedItems, value => value!.Contains("appsettings.Local.json", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(removedItems, value => value!.Contains("MockData.Local", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(removedItems, value => value!.Contains("wwwroot\\mock-data-local", StringComparison.OrdinalIgnoreCase));
    }

#if !INCLUDE_MOCK_BACKEND
    [Fact]
    public void DefaultReleasePublish_ExcludesMockRuntimeAndLocalAssets()
    {
        var repositoryRoot = RepositoryPath();
        var publishDirectory = Path.Combine(
            Path.GetTempPath(),
            $"mercurius-lan-release-{Guid.NewGuid():N}");

        Directory.CreateDirectory(publishDirectory);
        try
        {
            var publishResult = PublishReleaseArtifact(repositoryRoot, publishDirectory);
            Assert.True(
                publishResult.ExitCode == 0,
                $"Release publish failed with exit code {publishResult.ExitCode}.\n{publishResult.Output}");

            var assemblyPath = Path.Combine(publishDirectory, "Mercurius.LAN.Web.dll");
            Assert.True(File.Exists(assemblyPath), "The Release publish did not produce the web assembly.");

            var metadata = ReadPublishedMetadata(assemblyPath);
            Assert.DoesNotContain(
                metadata.TypeNames,
                typeName => typeName.StartsWith("Mercurius.LAN.Web.Mock.", StringComparison.Ordinal));
            Assert.DoesNotContain("MockBackendStore", metadata.TypeNames);
            Assert.DoesNotContain("MockTournamentService", metadata.TypeNames);
            Assert.DoesNotContain("MockAdminLoginHref", metadata.MemberNames);
            Assert.DoesNotContain("NormalizeMockPersona", metadata.MemberNames);
            Assert.DoesNotContain("BuildMockPrincipal", metadata.MemberNames);

            var publishedPaths = Directory
                .EnumerateFileSystemEntries(publishDirectory, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(publishDirectory, path).Replace('\\', '/'))
                .ToArray();

            Assert.DoesNotContain("appsettings.Local.json", publishedPaths, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(
                publishedPaths,
                path => path.Equals("MockData.Local", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("MockData.Local/", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(
                publishedPaths,
                path => path.Equals("wwwroot/mock-data-local", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("wwwroot/mock-data-local/", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if(Directory.Exists(publishDirectory))
                Directory.Delete(publishDirectory, recursive: true);
        }
    }
#endif

    private static IConfiguration BuildConfiguration(bool enabled) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{MockBackendOptions.SectionName}:Enabled"] = enabled.ToString()
            })
            .Build();

    private static string RepositoryPath(params string[] segments)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while(directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. segments]);
    }

#if !INCLUDE_MOCK_BACKEND
    private static ProcessResult PublishReleaseArtifact(string repositoryRoot, string publishDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = repositoryRoot,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("publish");
        startInfo.ArgumentList.Add(Path.Combine("src", "Mercurius.LAN.Web", "Mercurius.LAN.Web.csproj"));
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--output");
        startInfo.ArgumentList.Add(publishDirectory);

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        var standardOutput = process!.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        if(!process.WaitForExit(TimeSpan.FromMinutes(2)))
        {
            process.Kill(entireProcessTree: true);
            throw new Xunit.Sdk.XunitException("Release publish timed out after two minutes.");
        }

        Task.WaitAll(standardOutput, standardError);
        return new ProcessResult(
            process.ExitCode,
            $"{standardOutput.Result}\n{standardError.Result}");
    }

    private static PublishedMetadata ReadPublishedMetadata(string assemblyPath)
    {
        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        var metadataReader = peReader.GetMetadataReader();
        var typeNames = new List<string>();
        var memberNames = new List<string>();

        foreach(var typeHandle in metadataReader.TypeDefinitions)
        {
            var type = metadataReader.GetTypeDefinition(typeHandle);
            var typeName = metadataReader.GetString(type.Name);
            var typeNamespace = metadataReader.GetString(type.Namespace);
            typeNames.Add(string.IsNullOrEmpty(typeNamespace)
                ? typeName
                : $"{typeNamespace}.{typeName}");

            foreach(var methodHandle in type.GetMethods())
            {
                memberNames.Add(metadataReader.GetString(metadataReader.GetMethodDefinition(methodHandle).Name));
            }
        }

        return new PublishedMetadata(typeNames, memberNames);
    }

    private sealed record ProcessResult(int ExitCode, string Output);

    private sealed record PublishedMetadata(
        IReadOnlyCollection<string> TypeNames,
        IReadOnlyCollection<string> MemberNames);
#endif

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Mercurius.LAN.Web.ContractTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
