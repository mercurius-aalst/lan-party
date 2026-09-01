namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class CurrentUserTournamentRegistrationStateDTO
{
    public Guid TournamentId { get; init; }
    public TournamentRegistrationDTO? IndividualRegistration { get; init; }
    public TournamentRosterMemberDTO? PendingRosterConfirmation { get; init; }
    public TournamentRegistrationDTO? ActiveTeamRegistration { get; init; }
    public IReadOnlyList<TournamentRegistrationDTO> CaptainManagedRegistrations { get; init; } = [];
    public bool CanRegisterIndividual { get; init; }
    public bool CanConfirmRoster { get; init; }
    public bool CanUnregister { get; init; }
}
