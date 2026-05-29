namespace Mercurius.LAN.Web.Extensions;

public static class AssetUrlResolver
{
    public static string Resolve(IConfiguration configuration, string? assetPath)
    {
        if(string.IsNullOrWhiteSpace(assetPath))
            return string.Empty;

        if(Uri.TryCreate(assetPath, UriKind.Absolute, out _))
            return assetPath;

        if(assetPath.StartsWith("/", StringComparison.Ordinal))
            return assetPath;

        var baseAddress = configuration["MercuriusAPI:BaseAddress"];
        if(string.IsNullOrWhiteSpace(baseAddress))
            return "/" + assetPath.TrimStart('/');

        return $"{baseAddress.TrimEnd('/')}/{assetPath.TrimStart('/')}";
    }
}
