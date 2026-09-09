using System.Globalization;
using Microsoft.JSInterop;

namespace Mercurius.LAN.Web.Localization;

public interface ILocalizationService
{
    const string DefaultLanguage = "en-US";
    const string DutchLanguage = "nl-BE";
    const string LanguageCookieName = "mercurius-lan-language";

    string CurrentLanguage { get; }

    CultureInfo Culture { get; }

    IReadOnlyList<string> SupportedLanguages { get; }

    string this[string key] { get; }

    string Get(string key, params object?[] arguments);

    string FormatDate(DateTime value);

    string FormatDate(DateTimeOffset value);

    string FormatDateTime(DateTime value);

    string FormatDateTime(DateTimeOffset value);

    string FormatTime(DateTime value);

    string FormatTime(DateTimeOffset value);

    Task<bool> InitializeAsync(IJSRuntime jsRuntime, CancellationToken cancellationToken = default);

    Task<bool> SetLanguageAsync(IJSRuntime jsRuntime, string language, CancellationToken cancellationToken = default);
}
