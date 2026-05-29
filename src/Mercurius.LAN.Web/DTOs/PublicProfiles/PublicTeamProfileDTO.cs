namespace Mercurius.LAN.Web.DTOs.PublicProfiles;

public class PublicTeamProfileDTO
{
    public string TeamName { get; set; } = string.Empty;
    public string? CaptainUsername { get; set; }
    public IReadOnlyList<PublicTeamMemberDTO> Members { get; set; } = [];
    public IReadOnlyList<PublicTeamTournamentDTO> Tournaments { get; set; } = [];
}
