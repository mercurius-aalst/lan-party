using Mercurius.LAN.Web.Models.Auth;
using System.Security.Claims;

namespace Mercurius.LAN.Web.Services
{
    public interface IAuthService
    {
        ClaimsPrincipal CurrentUser { get; }
        bool IsLoggedIn { get; }

        event Action<ClaimsPrincipal>? UserChanged;
        Task<bool> GetStateFromTokenAsync(string accessToken, string refreshToken);
        Task Login(AuthTokenResponse tokens);
        Task LogoutAsync();
    }
}