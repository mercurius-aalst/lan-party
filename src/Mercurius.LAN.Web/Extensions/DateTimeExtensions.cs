using System.Globalization;

namespace Mercurius.LAN.Web.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToLocalDisplayTime(this DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Local => value,
            DateTimeKind.Utc => value.ToLocalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime()
        };
    }

    public static string ToUtcIsoString(this DateTime value)
    {
        var utcValue = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, value.Kind == DateTimeKind.Unspecified ? DateTimeKind.Local : value.Kind).ToUniversalTime();

        return utcValue.ToString("O", CultureInfo.InvariantCulture);
    }
}
