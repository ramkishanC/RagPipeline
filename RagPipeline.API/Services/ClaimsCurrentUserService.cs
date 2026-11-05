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
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            // Since MapInboundClaims = false, the claim name remains "sub"
            return user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");
        }
    }
}
