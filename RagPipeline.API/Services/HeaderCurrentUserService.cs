using RagPipeline.Application.Interfaces.Services;

namespace RagPipeline.API.Services
{
    public class HeaderCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context is null || !context.Request.Headers.TryGetValue("X-User-Id", out var userIdValues))
            {
                return null;
            }

            var userId = userIdValues.FirstOrDefault();
            return string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
        }
    }
}
