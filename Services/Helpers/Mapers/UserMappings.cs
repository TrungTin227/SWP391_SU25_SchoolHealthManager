using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Services.Helpers.Mapers
{
    public static class UserMappings
    {
        // Request to Domain mappings
        public static User ToDomainUser(this UserRegisterRequest req)
        {
            return new User
            {
                FirstName = req.FirstName ?? string.Empty,
                LastName = req.LastName ?? string.Empty,
                Email = req.Email,
                Gender = req.Gender?.ToString() ?? Gender.Other.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static User ToDomainUser(this AdminCreateUserRequest req)
        {
            return new User
            {
                FirstName = req.FirstName ?? string.Empty,
                LastName = req.LastName ?? string.Empty,
                Email = req.Email,
                Gender = req.Gender ?? Gender.Other.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyUpdate(this UpdateUserRequest req, User user)
        {
            if (!string.IsNullOrEmpty(req.FirstName)) user.FirstName = req.FirstName;
            if (!string.IsNullOrEmpty(req.LastName)) user.LastName = req.LastName;
            if (new EmailAddressAttribute().IsValid(req.Email)) user.Email = req.Email;
            if (!string.IsNullOrEmpty(req.PhoneNumbers)) user.PhoneNumber = req.PhoneNumbers;
            user.Gender = req.Gender.ToString();  // Gender is non-nullable enum
            user.UpdatedAt = DateTime.UtcNow;
        }

        // Domain to Response mappings
        public static async Task<UserResponse> ToUserResponseAsync(
            this User user,
            UserManager<User> userManager,
            string? accessToken = default,
            string? refreshToken = default)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender,
                PhoneNumbers = user.PhoneNumber ?? string.Empty,
                CreateAt = user.CreatedAt,
                UpdateAt = user.UpdatedAt,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Roles = roles.ToList()
            };
        }

        public static CurrentUserResponse ToCurrentUserResponse(this User user, string? accessToken = default)
        {
            return new CurrentUserResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Gender = user.Gender,
                PhoneNumbers = user.PhoneNumber ?? string.Empty,
                CreateAt = user.CreatedAt,
                UpdateAt = user.UpdatedAt,
                AccessToken = accessToken
            };
        }
    }
}