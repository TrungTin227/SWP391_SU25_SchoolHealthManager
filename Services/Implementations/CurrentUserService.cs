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
        public string? GetUserFullName()
        {
            // Lấy từ claim "FullName" hoặc ghép từ "FirstName" và "LastName"
            // Ví dụ:
            var firstName = _httpContextAccessor.HttpContext?.User?.FindFirst("FirstName")?.Value;
            var lastName = _httpContextAccessor.HttpContext?.User?.FindFirst("LastName")?.Value;
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return null;
            return $"{firstName} {lastName}".Trim();
        }
    }
}
