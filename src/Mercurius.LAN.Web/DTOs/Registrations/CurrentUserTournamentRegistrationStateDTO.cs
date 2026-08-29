namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class CurrentUserTournamentRegistrationStateDTO
{
    public Guid TournamentId { get; init; }
    public TournamentRegistrationDTO? IndividualRegistration { get; init; }
    public TournamentRosterMemberDTO? PendingRosterConfirmation { get; init; }
    /// <summary>
    /// The caller's team registration in any roster state. This is distinct from
    /// <see cref="ActiveTeamRegistration"/>, which remains active-only for compatibility.
    /// </summary>
    public TournamentRegistrationDTO? CurrentTeamRegistration { get; init; }
    public TournamentRegistrationDTO? ActiveTeamRegistration { get; init; }
    public IReadOnlyList<TournamentRegistrationDTO> CaptainManagedRegistrations { get; init; } = [];
    public bool CanRegisterIndividual { get; init; }
    public bool CanConfirmRoster { get; init; }
    public bool CanUnregister { get; init; }
}
