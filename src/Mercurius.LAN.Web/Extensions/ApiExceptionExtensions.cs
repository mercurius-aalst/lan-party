using System.Text.Json;
using Refit;

namespace Mercurius.LAN.Web.Extensions;

public static class ApiExceptionExtensions
{
    public sealed record ApiErrorDetails(string? Code, string? Message);

    public static ApiErrorDetails? GetApiError(this ApiException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var content = exception.Content;
        if(string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if(root.ValueKind == JsonValueKind.Object)
            {
                return new ApiErrorDetails(
                    GetStringProperty(root, "code"),
                    GetStringProperty(root, "message"));
            }

            if(root.ValueKind == JsonValueKind.String)
                return new ApiErrorDetails(null, root.GetString());
        }
        catch(JsonException)
        {
            // Refit can expose a plain-text body when a proxy or legacy endpoint responds.
        }

        return new ApiErrorDetails(null, content.Trim().Trim('"'));
    }

    private static string? GetStringProperty(JsonElement root, string propertyName) =>
        root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}
