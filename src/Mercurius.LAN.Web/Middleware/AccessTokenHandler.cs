using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Mercurius.LAN.Web.Middleware
{
    public class AccessTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if(httpContext == null)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var accessToken = await httpContext.GetTokenAsync("access_token");

            if(!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }     

            // Leave non-success responses intact so Refit can preserve the status code and
            // response body for the caller's unauthorized/error-state handling.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
