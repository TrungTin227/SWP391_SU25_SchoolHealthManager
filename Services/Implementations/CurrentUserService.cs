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
        public string? GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
        public bool IsAdmin()
        {
            // Giả sử role admin có tên "ADMIN"
            return _httpContextAccessor.HttpContext?.User?.IsInRole("ADMIN") ?? false;
        }
    }
}
