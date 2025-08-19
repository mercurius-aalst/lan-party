using System.Text.Json.Serialization;

namespace Mercurius.LAN.Web.Models.Participants
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(Team), typeDiscriminator: nameof(ParticipantType.Team))]
    [JsonDerivedType(typeof(Player), typeDiscriminator: nameof(ParticipantType.Player))]
    public abstract class Participant
    {
        [JsonIgnore]
        public abstract ParticipantType Type { get;}
  
        public int Id { get; set; }
    }
}