namespace Mercurius.LAN.Web.DTOs.Search;

using System.Text.Json.Serialization;

public enum GlobalSearchResultType
{
    [JsonStringEnumMemberName("user")]
    User,

    [JsonStringEnumMemberName("team")]
    Team,

    [JsonStringEnumMemberName("tournament")]
    Tournament
}
