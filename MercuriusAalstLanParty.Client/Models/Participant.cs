using System.Text.Json.Serialization;

namespace MercuriusAalstLanParty.Client.Models
{
    [JsonDerivedType(typeof(Player))]
    [JsonDerivedType(typeof(Team))]
    public abstract class Participant
    {
        public int Id { get; set; }
    }
}
