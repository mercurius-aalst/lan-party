using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Sponsors;
using Mercurius.LAN.Web.Models.Sponsors;
using System.Net.Http;

namespace Mercurius.LAN.Web.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ILANClient _lanClient;
        private readonly IConfiguration _configuration;

        public SponsorService(ILANClient lanClient, IConfiguration configuration)
        {
            _lanClient = lanClient;
            _configuration = configuration;
        }

        public async Task<IEnumerable<Sponsor>> GetSponsorsAsync()
        {
            var sponsors = await _lanClient.GetSponsorsAsync();
            sponsors.ToList().ForEach(sp => sp.LogoUrl = _configuration["MercuriusAPI:BaseAddress"] + sp.LogoUrl);

            return sponsors;
        }

        public Task<Sponsor> GetSponsorByIdAsync(int id)
        {
            return _lanClient.GetSponsorByIdAsync(id);
        }

        public async Task<Sponsor> CreateSponsorAsync(SponsorManagementDTO createSponsorDTO,string? tempFilePath,string? contentType,string? fileName)
        {
            var createSponsorFormData = new MultipartFormDataContent
                {
                    { new StringContent(createSponsorDTO.Name), "Name" },
                    { new StringContent(createSponsorDTO.InfoUrl), "InfoUrl" },
                    { new StringContent(createSponsorDTO.SponsorTier.ToString()), "SponsorTier" },
                };

            bool tempFileNeedsCleanup = false;

            if(!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                var fileStream = File.OpenRead(tempFilePath);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType!);
                createSponsorFormData.Add(streamContent, "Logo", fileName!);

                tempFileNeedsCleanup = true;
            }

            try
            {
                return await _lanClient.CreateSponsorAsync(createSponsorFormData);
            }
            finally
            {
                if(tempFileNeedsCleanup)
                {
                    try
                    {
                        File.Delete(tempFilePath!);
                    }
                    catch(Exception)
                    {
                    }
                }
            }
        }

        public async Task<Sponsor> UpdateSponsorAsync(int id, SponsorManagementDTO updateSponsorDTO, string? tempFilePath, string? contentType, string? fileName)
        {
            var updateSponsorFormData = new MultipartFormDataContent
                {
                    { new StringContent(updateSponsorDTO.Name), "Name" },
                    { new StringContent(updateSponsorDTO.InfoUrl), "InfoUrl" },
                    { new StringContent(updateSponsorDTO.SponsorTier.ToString()), "SponsorTier" },
                };

            bool tempFileNeedsCleanup = false;

            if(!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                var fileStream = File.OpenRead(tempFilePath);
                var streamContent = new StreamContent(fileStream);

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType!);
                updateSponsorFormData.Add(streamContent, "Logo", fileName!);

                tempFileNeedsCleanup = true;
            }

            try
            {
                return await _lanClient.UpdateSponsorAsync(id, updateSponsorFormData);
            }
            finally
            {
                if(tempFileNeedsCleanup)
                {
                    try
                    {
                        File.Delete(tempFilePath!);
                    }
                    catch(Exception)
                    {
                    }
                }
            }
        }
        public Task DeleteSponsorAsync(int id)
        {
            return _lanClient.DeleteSponsorAsync(id);
        }
    }
}