using Mercurius.LAN.Web.DTOs.Users;
using Mercurius.LAN.Web.Models.Tournaments;
using Mercurius.LAN.Web.Models.Participants;

namespace Mercurius.LAN.Web.Components.Shared;

public sealed class TournamentParticipantLookup
{
    public static readonly TournamentParticipantLookup Empty = new(
        new Dictionary<Guid, PublicUserDTO>(),
        new Dictionary<Guid, Team>());

    private readonly IReadOnlyDictionary<Guid, PublicUserDTO> _usersById;
    private readonly IReadOnlyDictionary<Guid, Team> _teamsById;

    private TournamentParticipantLookup(
        IReadOnlyDictionary<Guid, PublicUserDTO> usersById,
        IReadOnlyDictionary<Guid, Team> teamsById)
    {
        _usersById = usersById;
        _teamsById = teamsById;
    }

    public static TournamentParticipantLookup FromTournament(TournamentExtended? tournament)
    {
        if(tournament == null)
            return Empty;

        var usersById = tournament.Users
            .GroupBy(user => user.Id)
            .ToDictionary(group => group.Key, group => group.First());

        var teamsById = tournament.Teams
            .GroupBy(team => team.Id)
            .ToDictionary(group => group.Key, group => group.First());

        return new TournamentParticipantLookup(usersById, teamsById);
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
