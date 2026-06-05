using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Games;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Components.Shared;

public sealed class GameParticipantLookup
{
    public static readonly GameParticipantLookup Empty = new(
        new Dictionary<Guid, PublicUserDTO>(),
        new Dictionary<Guid, Team>());

    private readonly IReadOnlyDictionary<Guid, PublicUserDTO> _usersById;
    private readonly IReadOnlyDictionary<Guid, Team> _teamsById;

    private GameParticipantLookup(
        IReadOnlyDictionary<Guid, PublicUserDTO> usersById,
        IReadOnlyDictionary<Guid, Team> teamsById)
    {
        _usersById = usersById;
        _teamsById = teamsById;
    }

    public static GameParticipantLookup FromGame(GameExtended? game)
    {
        if(game == null)
            return Empty;

        var usersById = game.Users
            .GroupBy(user => user.Id)
            .ToDictionary(group => group.Key, group => group.First());

        var teamsById = game.Teams
            .GroupBy(team => team.Id)
            .ToDictionary(group => group.Key, group => group.First());

        return new GameParticipantLookup(usersById, teamsById);
    }

    public ParticipantViewModel? Resolve(ParticipationMode participationMode, Guid? participantId)
    {
        if(participantId is null)
            return null;

        return participationMode switch
        {
            ParticipationMode.Individual => _usersById.TryGetValue(participantId.Value, out var user)
                ? ParticipantViewModel.FromUser(user)
                : null,
            ParticipationMode.Team => _teamsById.TryGetValue(participantId.Value, out var team)
                ? ParticipantViewModel.FromTeam(team)
                : null,
            _ => null
        };
    }

    public string ResolveName(ParticipationMode participationMode, Guid? participantId)
    {
        return Resolve(participationMode, participantId)?.DisplayName ?? "TBD";
    }
}
