using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.Models.Sponsors;
using System.Net.Http;

namespace Mercurius.LAN.Web.Services
{
    public interface ISponsorService
    {
        Task<IEnumerable<Sponsor>> GetSponsorsAsync();
        Task<Sponsor> GetSponsorByIdAsync(int id);
        Task<Sponsor> CreateSponsorAsync(SponsorManagementDTO createSponsorDTO, string? tempFilePath, string? contentType, string? fileName);
        Task<Sponsor> UpdateSponsorAsync(int id, SponsorManagementDTO updateSponsorDTO, string? tempFilePath, string? contentType, string? fileName);
        Task DeleteSponsorAsync(int id);
    }
}