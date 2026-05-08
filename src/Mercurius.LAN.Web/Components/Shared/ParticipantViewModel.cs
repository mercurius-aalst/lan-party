using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Components.Shared;

public sealed class ParticipantViewModel
{
    public Guid Id { get; init; }
    public ParticipationMode ParticipationMode { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public UserDTO? User { get; init; }
    public Team? Team { get; init; }

    public static ParticipantViewModel FromUser(UserDTO user) => new()
    {
        Id = user.Id,
        ParticipationMode = ParticipationMode.Individual,
        DisplayName = user.Username ?? user.DisplayName,
        User = user
    };

    public static ParticipantViewModel FromTeam(Team team) => new()
    {
        Id = team.Id,
        ParticipationMode = ParticipationMode.Team,
        DisplayName = team.Name,
        Team = team
    };
}
