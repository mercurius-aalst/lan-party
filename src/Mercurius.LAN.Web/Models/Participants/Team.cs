using System.ComponentModel.DataAnnotations;

namespace Mercurius.LAN.Web.Models.Participants
{
    public class Team : Participant
    {
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public int CaptainId { get; set; }
      
        public IEnumerable<Player> Players { get; set; } = new List<Player>();
        public IEnumerable<TeamInvite> TeamInvites { get; set; } = new List<TeamInvite>();

        public override ParticipantType Type => ParticipantType.Team;
    }
}