using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;

namespace Mercurius.LAN.Web.Services
{
    public class AuthService : IAuthService
    {
        public AuthService()
        {
        }

        public ClaimsPrincipal CurrentUser { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

        public bool IsLoggedIn => CurrentUser.Identity?.IsAuthenticated ?? false;

        public Task LoginAsync(AuthTokenResponse tokens)
        {
            // Parse the JWT to extract claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(tokens.Token);

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            CurrentUser = new ClaimsPrincipal(identity);

            return Task.CompletedTask;
        }

        public Task LogoutAsync()
        {
            CurrentUser = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.CompletedTask;
        }

        public event Action<ClaimsPrincipal>? UserChanged;

    }
}