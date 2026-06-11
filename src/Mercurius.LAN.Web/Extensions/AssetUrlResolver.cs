namespace Mercurius.LAN.Web.Extensions;

public static class AssetUrlResolver
{
    public static string Resolve(IConfiguration configuration, string? assetPath)
    {
        if(string.IsNullOrWhiteSpace(assetPath))
            return string.Empty;

        if(Uri.TryCreate(assetPath, UriKind.Absolute, out _))
            return assetPath;

        if(IsFrontendAssetPath(assetPath))
            return assetPath;

        var baseAddress = configuration["MercuriusAPI:BaseAddress"];
        if(string.IsNullOrWhiteSpace(baseAddress))
            return assetPath.StartsWith("/", StringComparison.Ordinal) ? assetPath : "/" + assetPath.TrimStart('/');

        return $"{baseAddress.TrimEnd('/')}/{assetPath.TrimStart('/')}";
    }

    private static bool IsFrontendAssetPath(string assetPath) =>
        assetPath.StartsWith("/mock-data-local/", StringComparison.OrdinalIgnoreCase)
        || assetPath.StartsWith("/img/", StringComparison.OrdinalIgnoreCase)
        || string.Equals(assetPath, "/favicon.svg", StringComparison.OrdinalIgnoreCase);
}
