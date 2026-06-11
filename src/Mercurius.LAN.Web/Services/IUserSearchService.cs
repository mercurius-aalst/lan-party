using Mercurius.LAN.Web.DTOs.Search;

namespace Mercurius.LAN.Web.Services;

public interface IUserSearchService
{
    Task<IReadOnlyList<GlobalSearchResultDTO>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
