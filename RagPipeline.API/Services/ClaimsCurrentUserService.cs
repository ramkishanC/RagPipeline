using RagPipeline.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RagPipeline.API.Services
{
    public class ClaimsCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var subClaim = httpContext.User.FindFirstValue("sub")
                        ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrWhiteSpace(subClaim))
            {
                return subClaim;
            }

            if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var headerUserId) &&
                !string.IsNullOrWhiteSpace(headerUserId))
            {
                return headerUserId.ToString();
            }

            return null;
        }
    }
}
