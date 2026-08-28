using Mercurius.LAN.Web.Models.Tournaments;

namespace Mercurius.LAN.Web.DTOs.Registrations;

public sealed class UpdateTournamentLifecycleStateRequestDTO
{
    public TournamentStatus? State { get; set; }
}
