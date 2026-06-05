using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Matches;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Models.Games
{
    public class GameExtended : Game
    {
        public IEnumerable<Placement> Placements { get; set; } = new List<Placement>();
        public IEnumerable<Match> Matches { get; set; } = new List<Match>();
        public IEnumerable<PublicUserDTO> Users { get; set; } = new List<PublicUserDTO>();
        public IEnumerable<Team> Teams { get; set; } = new List<Team>();
        public GameSponsorPlacement? SponsorPlacement { get; set; }
        public IEnumerable<GameSponsorPlacement> SponsorPlacements
        {
            get => SponsorPlacement is null ? [] : [SponsorPlacement];
            set => SponsorPlacement = value?.FirstOrDefault();
        }
    }
}
