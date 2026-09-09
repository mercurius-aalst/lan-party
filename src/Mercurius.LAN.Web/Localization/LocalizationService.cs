using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private const string ResourceDirectory = "locales";

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalizationService> _logger;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly Dictionary<string, string> _englishResources;
    private readonly HashSet<string> _reportedMissingKeys = new(StringComparer.Ordinal);
    private Dictionary<string, string> _activeResources;

    public LocalizationService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalizationService> logger,
        IHostEnvironment hostEnvironment)
    {
        _environment = environment;
        _logger = logger;
        _hostEnvironment = hostEnvironment;

        CurrentLanguage = NormalizeLanguage(httpContextAccessor.HttpContext?.Request.Cookies[ILocalizationService.LanguageCookieName])
            ?? ILocalizationService.DefaultLanguage;
        _englishResources = LoadResources(ILocalizationService.DefaultLanguage);
        _activeResources = string.Equals(CurrentLanguage, ILocalizationService.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
            ? _englishResources
            : LoadResources(CurrentLanguage);
    }

    public string CurrentLanguage { get; private set; }

    public CultureInfo Culture => CultureInfo.GetCultureInfo(CurrentLanguage);

    public IReadOnlyList<string> SupportedLanguages { get; } =
        [ILocalizationService.DefaultLanguage, ILocalizationService.DutchLanguage];

    public string this[string key] => Get(key);

    public string Get(string key, params object?[] arguments)
    {
        if(string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var value = Resolve(key);
        return arguments.Length == 0
            ? value
            : string.Format(Culture, value, arguments);
    }

    public string FormatDate(DateTime value) => value.ToString("d", Culture);

    public string FormatDate(DateTimeOffset value) => value.ToString("d", Culture);

    public string FormatDateTime(DateTime value) => value.ToString("g", Culture);

    public string FormatDateTime(DateTimeOffset value) => value.ToString("g", Culture);

    public string FormatTime(DateTime value) => value.ToString("t", Culture);

    public string FormatTime(DateTimeOffset value) => value.ToString("t", Culture);

    public async Task<bool> InitializeAsync(IJSRuntime jsRuntime, CancellationToken cancellationToken = default)
    {
        try
        {
            var storedLanguage = await jsRuntime.InvokeAsync<string?>("lanLocalization.getStored", cancellationToken);
            var normalizedLanguage = NormalizeLanguage(storedLanguage);
            if(normalizedLanguage is null || string.Equals(normalizedLanguage, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            {
                await jsRuntime.InvokeVoidAsync("lanLocalization.apply", cancellationToken, CurrentLanguage);
                return false;
            }

            SetCurrentLanguage(normalizedLanguage);
            return await jsRuntime.InvokeAsync<bool>("lanLocalization.syncCookie", cancellationToken, CurrentLanguage);
        }
        catch(JSException)
        {
            return false;
        }
        catch(InvalidOperationException)
        {
            // JS interop is unavailable during static prerendering. The cookie/default remains valid.
            return false;
        }
    }

    public async Task<bool> SetLanguageAsync(IJSRuntime jsRuntime, string language, CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = NormalizeLanguage(language);
        if(normalizedLanguage is null)
            return false;

        if(string.Equals(normalizedLanguage, CurrentLanguage, StringComparison.OrdinalIgnoreCase))
        {
            await jsRuntime.InvokeVoidAsync("lanLocalization.apply", cancellationToken, CurrentLanguage);
            return false;
        }

        SetCurrentLanguage(normalizedLanguage);
        return await jsRuntime.InvokeAsync<bool>("lanLocalization.set", cancellationToken, CurrentLanguage);
    }

    private string Resolve(string key)
    {
        if(_activeResources.TryGetValue(key, out var activeValue))
            return activeValue;

        if(_englishResources.TryGetValue(key, out var englishValue))
        {
            ReportMissingKey(key);
            return englishValue;
        }

        ReportMissingKey(key);
        return $"[[{key}]]";
    }

    private void ReportMissingKey(string key)
    {
        if(!_hostEnvironment.IsDevelopment() || !_reportedMissingKeys.Add(key))
            return;

        _logger.LogWarning("Missing {Language} localization key: {Key}", CurrentLanguage, key);
    }

    private void SetCurrentLanguage(string language)
    {
        CurrentLanguage = language;
        _activeResources = string.Equals(language, ILocalizationService.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
            ? _englishResources
            : LoadResources(language);
    }

    private Dictionary<string, string> LoadResources(string language)
    {
        var resourcePath = Path.Combine(_environment.WebRootPath, ResourceDirectory, $"translations.{language}.json");
        if(!File.Exists(resourcePath))
        {
            _logger.LogWarning("Localization resource file was not found: {Path}", resourcePath);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        try
        {
            var json = File.ReadAllText(resourcePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch(JsonException exception)
        {
            _logger.LogError(exception, "Localization resource file is invalid: {Path}", resourcePath);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch(IOException exception)
        {
            _logger.LogError(exception, "Localization resource file could not be read: {Path}", resourcePath);
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    private static string? NormalizeLanguage(string? language)
    {
        if(string.Equals(language, ILocalizationService.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            return ILocalizationService.DefaultLanguage;

        if(string.Equals(language, ILocalizationService.DutchLanguage, StringComparison.OrdinalIgnoreCase))
            return ILocalizationService.DutchLanguage;

        return null;
    }
}
