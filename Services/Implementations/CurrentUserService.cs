using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Services.Implementations
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid? GetUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return null;

            if (Guid.TryParse(userIdString, out var userId))
                return userId;

            return null;
        }
        public bool IsAdmin()
        {
            // Giả sử role admin có tên "ADMIN"
            return _httpContextAccessor.HttpContext?.User?.IsInRole("ADMIN") ?? false;
        }
    }
}
