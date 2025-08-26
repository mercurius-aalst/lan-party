namespace Mercurius.LAN.Web.DTOs.Participants.Teams
{
    public class CreateTeamDTO
    {
        public string Name { get; set; } = null!;
        public int CaptainId { get; set; }
    }
}
