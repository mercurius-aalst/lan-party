using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;

namespace Mercurius.LAN.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthenticationClient _authClient;
        private readonly IConfiguration _configuration;

        public AuthService(IAuthenticationClient authClient, IConfiguration configuration)
        {
            _authClient = authClient;
            _configuration = configuration;
        }

        public ClaimsPrincipal CurrentUser { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

        public bool IsLoggedIn => CurrentUser.Identity?.IsAuthenticated ?? false;

        public async Task<bool> GetStateFromTokenAsync(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
                    var newTokens = await _authClient.RefreshAsync(refreshRequest);

                    accessToken = newTokens.Token;
                    refreshToken = newTokens.RefreshToken;

                    // Return the new tokens to the caller for storage in cookies
                    return true;
                }
                catch
                {
                    await LogoutAsync();
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

                    tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = _configuration["Jwt:Audience"],
                        ValidIssuer = _configuration["Jwt:Issuer"]
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    CurrentUser = new ClaimsPrincipal(identity);

                    return true;
                }
                catch
                {
                    await LogoutAsync();
                }
            }

            return false;
        }

        public Task Login(AuthTokenResponse tokens)
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