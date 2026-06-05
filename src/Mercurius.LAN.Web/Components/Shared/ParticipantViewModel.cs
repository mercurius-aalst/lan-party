using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Components.Shared;

public sealed class ParticipantViewModel
{
    public Guid Id { get; init; }
    public ParticipationMode ParticipationMode { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public PublicUserDTO? User { get; init; }
    public Team? Team { get; init; }

    public static ParticipantViewModel FromUser(PublicUserDTO user) => new()
    {
        Id = user.Id,
        ParticipationMode = ParticipationMode.Individual,
        DisplayName = GetUserDisplayName(user),
        User = user
    };

    public static ParticipantViewModel FromUser(UserDTO user) =>
        FromUser(PublicUserDTO.FromUser(user));

    public static ParticipantViewModel FromTeam(Team team) => new()
    {
        Id = team.Id,
        ParticipationMode = ParticipationMode.Team,
        DisplayName = team.Name,
        Team = team
    };

    private static string GetUserDisplayName(PublicUserDTO user)
    {
        if(!string.IsNullOrWhiteSpace(user.Username))
            return user.Username.Trim();

        if(!string.IsNullOrWhiteSpace(user.DisplayName))
            return user.DisplayName.Trim();

        var fullName = string.Join(" ", new[] { user.Firstname, user.Lastname }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim()));

        return string.IsNullOrWhiteSpace(fullName) ? "Participant" : fullName;
    }
}
