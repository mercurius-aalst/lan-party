using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mercurius.LAN.Web.APIClients;
using Mercurius.LAN.Web.Models.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace Mercurius.LAN.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public AuthService(AuthenticationStateProvider authenticationStateProvider)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _authenticationStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
            var authStateTask = _authenticationStateProvider.GetAuthenticationStateAsync();
            CurrentUser = authStateTask.Result.User;
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

        private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var authState = task.GetAwaiter().GetResult();
            CurrentUser = authState.User;
            // You can now raise your UserChanged event here if needed
            UserChanged?.Invoke(CurrentUser);
        }
    }
}