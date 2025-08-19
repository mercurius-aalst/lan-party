using Mercurius.LAN.Web.Models.Auth;
using Refit;

namespace Mercurius.LAN.Web.APIClients
{
    public interface IAuthenticationClient
    {
        [Post("/auth/register")]
        Task RegisterAsync([Body] LoginRequest request);

        [Post("/auth/login")]
        Task<AuthTokenResponse> LoginAsync([Body] LoginRequest request);

        [Post("/auth/refresh")]
        Task<AuthTokenResponse> RefreshAsync([Body] RefreshTokenRequest request);

        [Post("/auth/revoke")]
        Task RevokeAsync([Body] RevokeTokenRequest request);
    }
}
