using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.Models.Sponsors;
using System.Net.Http;

namespace Mercurius.LAN.Web.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ILANClient _lanClient;

        public SponsorService(ILANClient lanClient)
        {
            _lanClient = lanClient;
        }

        public Task<IEnumerable<Sponsor>> GetSponsorsAsync()
        {
            return _lanClient.GetSponsorsAsync();
        }

        public Task<Sponsor> GetSponsorByIdAsync(int id)
        {
            return _lanClient.GetSponsorByIdAsync(id);
        }

        public Task<Sponsor> CreateSponsorAsync(SponsorManagementDTO createSponsorDTO)
        {
            var createSponsorFormData = new MultipartFormDataContent
            {
                { new StringContent(createSponsorDTO.Name), "Name" },
                { new StringContent(createSponsorDTO.InfoUrl), "InfoUrl" },
                {new StringContent(createSponsorDTO.SponsorTier.ToString()), "SponsorTier" },
            };

            if (createSponsorDTO.Logo != null)
            {
                var streamContent = new StreamContent(createSponsorDTO.Logo.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(createSponsorDTO.Logo.ContentType);
                createSponsorFormData.Add(streamContent, "Logo", createSponsorDTO.Logo.Name);
            }
            return _lanClient.CreateSponsorAsync(createSponsorFormData);
        }

        public Task<Sponsor> UpdateSponsorAsync(int id, SponsorManagementDTO updateSponsorDTO)
        {
            var updateSponsorFormData = new MultipartFormDataContent
            {
                { new StringContent(updateSponsorDTO.Name), "Name" },
                { new StringContent(updateSponsorDTO.InfoUrl), "InfoUrl" },
                { new StringContent(updateSponsorDTO.SponsorTier.ToString()), "SponsorTier" },
            };
            if (updateSponsorDTO.Logo != null)
            {
                var streamContent = new StreamContent(updateSponsorDTO.Logo.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(updateSponsorDTO.Logo.ContentType);
                updateSponsorFormData.Add(streamContent, "Logo", updateSponsorDTO.Logo.Name);
            }
            return _lanClient.UpdateSponsorAsync(id, updateSponsorFormData);
        }

        public Task DeleteSponsorAsync(int id)
        {
            return _lanClient.DeleteSponsorAsync(id);
        }
    }

}