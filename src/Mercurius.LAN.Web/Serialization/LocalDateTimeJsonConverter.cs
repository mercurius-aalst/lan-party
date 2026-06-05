using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mercurius.LAN.Web.Extensions;

namespace Mercurius.LAN.Web.Serialization;

public sealed class LocalDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if(reader.TokenType != JsonTokenType.String)
            return reader.GetDateTime().ToLocalDisplayTime();

        var value = reader.GetString();
        if(string.IsNullOrWhiteSpace(value))
            return default;

        if(HasExplicitOffset(value) &&
           DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffset))
        {
            return dateTimeOffset.LocalDateTime;
        }

        var dateTime = DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime,
            DateTimeKind.Utc => dateTime.ToLocalDisplayTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalDisplayTime()
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUtcIsoString());
    }

    private static bool HasExplicitOffset(string value)
    {
        if(value.EndsWith('Z'))
            return true;

        var timeSeparatorIndex = value.IndexOf('T');
        if(timeSeparatorIndex < 0)
            timeSeparatorIndex = value.IndexOf(' ');

        if(timeSeparatorIndex < 0)
            return false;

        return value.IndexOf('+', timeSeparatorIndex) >= 0 ||
            value.IndexOf('-', timeSeparatorIndex) >= 0;
    }
}
