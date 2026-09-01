using Mercurius.LAN.Web.DTOs.Participants.Teams;
using Mercurius.LAN.Web.DTOs.PublicProfiles;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Extensions;

public static class TeamAssetUrlResolver
{
    public static void Resolve(IConfiguration configuration, Team team)
    {
        team.LogoUrl = ResolveLogoUrl(configuration, team.LogoUrl);
    }

    public static void Resolve(IConfiguration configuration, TeamManagementSummaryDTO team)
    {
        team.LogoUrl = ResolveLogoUrl(configuration, team.LogoUrl);
    }

    public static void Resolve(IConfiguration configuration, PublicTeamProfileDTO team)
    {
        team.LogoUrl = ResolveLogoUrl(configuration, team.LogoUrl);
    }

    public static void Resolve(IConfiguration configuration, TeamLogoResponseDTO response)
    {
        response.LogoUrl = ResolveLogoUrl(configuration, response.LogoUrl);
    }

    public static void Resolve(IConfiguration configuration, TeamInviteSummaryDTO invite)
    {
        invite.TeamLogoUrl = ResolveLogoUrl(configuration, invite.TeamLogoUrl);
    }

    public static void Resolve(IConfiguration configuration, CurrentUserTeamSummaryDTO summary)
    {
        foreach(var team in summary.CaptainedTeams)
            Resolve(configuration, team);

        foreach(var team in summary.MemberTeams)
            Resolve(configuration, team);

        foreach(var invite in summary.ReceivedPendingInvites)
            Resolve(configuration, invite);

        foreach(var invite in summary.SentPendingInvites)
            Resolve(configuration, invite);
    }

    public static void Resolve(IConfiguration configuration, TournamentExtended tournament)
    {
        foreach(var registration in tournament.Registrations)
        {
            if(registration.Team is not null)
                registration.Team.LogoUrl = ResolveLogoUrl(configuration, registration.Team.LogoUrl);
        }

        foreach(var team in tournament.Teams)
            Resolve(configuration, team);

        foreach(var placement in tournament.Placements)
        {
            foreach(var team in placement.Teams)
                Resolve(configuration, team);
        }
    }

    private static string? ResolveLogoUrl(IConfiguration configuration, string? logoUrl) =>
        string.IsNullOrWhiteSpace(logoUrl) ? null : AssetUrlResolver.Resolve(configuration, logoUrl);
}
