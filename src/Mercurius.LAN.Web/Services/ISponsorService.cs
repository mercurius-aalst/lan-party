using Mercurius.LAN.Web.Models.Sponsors;
using System.Net.Http;

namespace Mercurius.LAN.Web.Services
{
    public interface ISponsorService
    {
        Task<IEnumerable<Sponsor>> GetSponsorsAsync();
        Task<Sponsor> GetSponsorByIdAsync(int id);
        Task<Sponsor> CreateSponsorAsync(MultipartFormDataContent createSponsorFormData);
        Task<Sponsor> UpdateSponsorAsync(int id, MultipartFormDataContent updateSponsorFormData);
    }
}