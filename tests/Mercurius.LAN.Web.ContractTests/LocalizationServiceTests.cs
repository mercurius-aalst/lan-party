using System.Text.Json;
using Mercurius.LAN.Web.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Xunit;

namespace Mercurius.LAN.Web.ContractTests;

public sealed class LocalizationServiceTests
{
    [Fact]
    public void LocaleResourcesHaveMatchingKeys()
    {
        var english = ReadResource("translations.en-US.json");
        var dutch = ReadResource("translations.nl-BE.json");

        Assert.NotEmpty(english);
        Assert.Equal(
            english.Keys.OrderBy(static key => key),
            dutch.Keys.OrderBy(static key => key));
        Assert.All(english, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Value), $"English key '{entry.Key}' is empty."));
        Assert.All(dutch, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Value), $"Dutch key '{entry.Key}' is empty."));
    }

    [Fact]
    public void LanguageBootstrapPersistsSupportedLocaleAndUpdatesDocumentLanguage()
    {
        var appMarkup = File.ReadAllText(FindRepositoryFile(Path.Combine("src", "Mercurius.LAN.Web", "Components", "App.razor")));

        Assert.Contains("const languageStorageKey = \"mercurius-lan-language\"", appMarkup);
        Assert.Contains("const languageCookieName = \"mercurius-lan-language\"", appMarkup);
        Assert.Contains("new Set([\"en-US\", \"nl-BE\"])", appMarkup);
        Assert.Contains("localStorage.getItem(languageStorageKey)", appMarkup);
        Assert.Contains("localStorage.setItem(languageStorageKey, normalizedLanguage)", appMarkup);
        Assert.Contains("document.cookie", appMarkup);
        Assert.Contains("document.documentElement.lang = normalizedLanguage", appMarkup);
    }

    [Fact]
    public void DutchResourceFallsBackToEnglishAndUsesDutchDateCulture()
    {
        using var resources = TestResources.Create(
            new Dictionary<string, string>
            {
                ["greeting"] = "Hello",
                ["fallback"] = "English fallback"
            },
            new Dictionary<string, string>
            {
                ["greeting"] = "Hallo"
            });
        var service = resources.CreateService(ILocalizationService.DutchLanguage);

        Assert.Equal(ILocalizationService.DutchLanguage, service.CurrentLanguage);
        Assert.Equal("Hallo", service["greeting"]);
        Assert.Equal("English fallback", service["fallback"]);

        var date = new DateTime(2026, 9, 8);
        Assert.Equal(date.ToString("d", service.Culture), service.FormatDate(date));
    }

    [Fact]
    public async Task InvalidStoredLanguageUsesEnglishAndSupportedSelectionIsPersisted()
    {
        using var resources = TestResources.Create(
            new Dictionary<string, string> { ["greeting"] = "Hello" },
            new Dictionary<string, string> { ["greeting"] = "Hallo" });
        var service = resources.CreateService();
        var jsRuntime = new RecordingJsRuntime("fr-FR");

        Assert.False(await service.InitializeAsync(jsRuntime));
        Assert.Equal(ILocalizationService.DefaultLanguage, service.CurrentLanguage);
        Assert.Contains(jsRuntime.Calls, call => call.Identifier == "lanLocalization.apply");

        Assert.False(await service.SetLanguageAsync(jsRuntime, "fr-FR"));
        Assert.True(await service.SetLanguageAsync(jsRuntime, ILocalizationService.DutchLanguage));
        Assert.Equal(ILocalizationService.DutchLanguage, service.CurrentLanguage);
        Assert.Contains(jsRuntime.Calls, call => call.Identifier == "lanLocalization.set");
    }

    [Fact]
    public async Task InitializeAsyncKeepsLocalLanguageWhenCookiePersistenceIsBlockedAcrossRequests()
    {
        using var resources = TestResources.Create(
            new Dictionary<string, string> { ["greeting"] = "Hello" },
            new Dictionary<string, string> { ["greeting"] = "Hallo" });
        var jsRuntime = new RecordingJsRuntime(ILocalizationService.DutchLanguage, cookiePersistenceSucceeded: false);

        var firstRequest = resources.CreateService();
        Assert.False(await firstRequest.InitializeAsync(jsRuntime));
        Assert.Equal(ILocalizationService.DutchLanguage, firstRequest.CurrentLanguage);
        Assert.Equal("Hallo", firstRequest["greeting"]);

        var secondRequest = resources.CreateService();
        Assert.False(await secondRequest.InitializeAsync(jsRuntime));
        Assert.Equal(ILocalizationService.DutchLanguage, secondRequest.CurrentLanguage);
        Assert.Equal("Hallo", secondRequest["greeting"]);
        Assert.Equal(2, jsRuntime.Calls.Count(call => call.Identifier == "lanLocalization.syncCookie"));
    }

    [Fact]
    public async Task InitializeAsyncRequestsReloadWhenCookiePersistenceSucceeds()
    {
        using var resources = TestResources.Create(
            new Dictionary<string, string> { ["greeting"] = "Hello" },
            new Dictionary<string, string> { ["greeting"] = "Hallo" });
        var service = resources.CreateService();
        var jsRuntime = new RecordingJsRuntime(ILocalizationService.DutchLanguage);

        Assert.True(await service.InitializeAsync(jsRuntime));
        Assert.Equal(ILocalizationService.DutchLanguage, service.CurrentLanguage);
    }

    private static Dictionary<string, string> ReadResource(string fileName)
    {
        var path = FindRepositoryFile(Path.Combine("src", "Mercurius.LAN.Web", "wwwroot", "locales", fileName));
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
            ?? throw new InvalidOperationException($"Could not parse '{path}'.");
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while(directory is not null)
        {
            var path = Path.Combine(directory.FullName, relativePath);
            if(File.Exists(path))
                return path;

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Could not locate repository file '{relativePath}'.");
    }

    private sealed class TestResources : IDisposable
    {
        private readonly string _rootPath;

        private TestResources(string rootPath)
        {
            _rootPath = rootPath;
        }

        public static TestResources Create(
            IReadOnlyDictionary<string, string> english,
            IReadOnlyDictionary<string, string> dutch)
        {
            var rootPath = Path.Combine(Path.GetTempPath(), $"mercurius-localization-{Guid.NewGuid():N}");
            var localePath = Path.Combine(rootPath, "locales");
            Directory.CreateDirectory(localePath);
            File.WriteAllText(Path.Combine(localePath, "translations.en-US.json"), JsonSerializer.Serialize(english));
            File.WriteAllText(Path.Combine(localePath, "translations.nl-BE.json"), JsonSerializer.Serialize(dutch));
            return new TestResources(rootPath);
        }

        public LocalizationService CreateService(string? language = null)
        {
            var httpContext = new DefaultHttpContext();
            if(!string.IsNullOrWhiteSpace(language))
                httpContext.Request.Headers.Cookie = $"{ILocalizationService.LanguageCookieName}={language}";

            var environment = new TestEnvironment(_rootPath);
            return new LocalizationService(
                environment,
                new HttpContextAccessor { HttpContext = httpContext },
                NullLogger<LocalizationService>.Instance,
                environment);
        }

        public void Dispose()
        {
            if(Directory.Exists(_rootPath))
                Directory.Delete(_rootPath, recursive: true);
        }
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public TestEnvironment(string rootPath)
        {
            ContentRootPath = rootPath;
            WebRootPath = rootPath;
            ContentRootFileProvider = new PhysicalFileProvider(rootPath);
            WebRootFileProvider = ContentRootFileProvider;
        }

        public string ApplicationName { get; set; } = typeof(LocalizationServiceTests).Assembly.GetName().Name!;
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = Environments.Development;
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
    }

    private sealed class RecordingJsRuntime : IJSRuntime
    {
        private readonly string _storedLanguage;
        private readonly bool _cookiePersistenceSucceeded;

        public RecordingJsRuntime(string storedLanguage, bool cookiePersistenceSucceeded = true)
        {
            _storedLanguage = storedLanguage;
            _cookiePersistenceSucceeded = cookiePersistenceSucceeded;
        }

        public List<(string Identifier, object?[]? Arguments)> Calls { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add((identifier, args));
            return ValueTask.FromResult(GetResult<TValue>(identifier));
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Calls.Add((identifier, args));
            return ValueTask.FromResult(GetResult<TValue>(identifier));
        }

        private TValue GetResult<TValue>(string identifier)
        {
            if(identifier == "lanLocalization.getStored")
                return (TValue)(object)_storedLanguage;

            if(identifier is "lanLocalization.syncCookie" or "lanLocalization.set")
                return (TValue)(object)_cookiePersistenceSucceeded;

            return default!;
        }
    }
}
