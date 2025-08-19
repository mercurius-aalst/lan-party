using System.Collections.Generic;

namespace Mercurius.LAN.Web.Models.Participants
{
    public class Team : Participant
    {
        public string Name { get; set; }
        public int CaptainId { get; set; }
        public IEnumerable<Player> Players { get; set; } = new List<Player>();
        public IEnumerable<TeamInvite> TeamInvites { get; set; } = new List<TeamInvite>();

        public override ParticipantType Type => ParticipantType.Team;
    }
}