using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.DTOs.Search;

namespace Mercurius.LAN.Web.Services;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly ILANClient _lanClient;

    public GlobalSearchService(ILANClient lanClient)
    {
        _lanClient = lanClient;
    }

    public async Task<IReadOnlyList<GlobalSearchResultDTO>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var trimmedQuery = query.Trim();
        if(trimmedQuery.Length < 3)
            return [];

        var response = await _lanClient.SearchAsync(trimmedQuery, cancellationToken: cancellationToken);
        return response.Results;
    }
}
