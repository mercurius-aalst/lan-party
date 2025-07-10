using System.Text.Json.Serialization;

namespace Mercurius.LAN.Web.Models
{
    [JsonDerivedType(typeof(Player))]
    [JsonDerivedType(typeof(Team))]
    public class Participant
    {
        public int Id { get; set; }
    }
}
