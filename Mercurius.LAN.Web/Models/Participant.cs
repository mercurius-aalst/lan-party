using System.Text.Json.Serialization;

namespace Mercurius.LAN.Web.Models
{
    [JsonDerivedType(typeof(Player), nameof(ParticipantType.Player))]
    [JsonDerivedType(typeof(Team), nameof(ParticipantType.Team))]
    public class Participant
    {
        [JsonPropertyName("$type")]
        public ParticipantType Type { get; set; }
        public int Id { get; set; }
    }
}
