using Mercurius.LAN.Web.DTOs.Search;

namespace Mercurius.LAN.Web.Services;

public interface IGlobalSearchService
{
    Task<SearchResponseDTO> SearchAsync(string query, CancellationToken cancellationToken = default);
}
