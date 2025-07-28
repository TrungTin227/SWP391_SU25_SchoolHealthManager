using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Text.Json;

namespace Services.Helpers
{
    public static class UserManagerExtensions
    {
        public static async Task<bool> ExistsByEmailAsync(
            this UserManager<User> mgr, string email)
            => await mgr.FindByEmailAsync(email) != null;

        public static Task AddDefaultRoleAsync(
            this UserManager<User> mgr, User user)
            => mgr.AddToRoleAsync(user, "Parent");

        public static Task AddRolesAsync(
            this UserManager<User> mgr, User user, IEnumerable<string> roles)
            => mgr.AddToRolesAsync(user, roles ?? new[] { "Parent" });

        
        public static async Task<IdentityResult> SetRefreshTokenAsync(
            this UserManager<User> mgr, User user, RefreshTokenInfo refreshTokenInfo)
        {
            var tokenJson = JsonSerializer.Serialize(refreshTokenInfo);
            return await mgr.SetAuthenticationTokenAsync(user, "SchoolHealthManager", "RefreshToken", tokenJson);
        }


        public static async Task<RefreshTokenInfo?> GetRefreshTokenAsync(this UserManager<User> mgr, User user)
        {
            var tokenJson = await mgr.GetAuthenticationTokenAsync(user, "SchoolHealthManager", "RefreshToken");
            if (string.IsNullOrEmpty(tokenJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<RefreshTokenInfo>(tokenJson);
            }
            catch
            {
                return null;
            }
        }

        // CẬP NHẬT: Thay đổi method này để validate với expiry time
        public static async Task<bool> ValidateRefreshTokenAsync(
            this UserManager<User> mgr, User user, string token)
        {
            var storedTokenInfo = await mgr.GetRefreshTokenAsync(user);

            if (storedTokenInfo == null)
                return false;

            // Kiểm tra token có match không
            if (storedTokenInfo.Token != token)
                return false;

            // Kiểm tra token có expired không
            if (storedTokenInfo.Expiry <= DateTime.UtcNow)
                return false;

            return true;
        }

        public static Task ResetAccessFailedAsync(
            this UserManager<User> mgr, User user)
            => mgr.ResetAccessFailedCountAsync(user);

        public static async Task<IdentityResultWrapper> RemoveRefreshTokenAsync(
            this UserManager<User> mgr, User user)
        {
            var res = await mgr.RemoveAuthenticationTokenAsync(
                user, "SchoolHealthManager", "RefreshToken");
            return new IdentityResultWrapper(res);
        }

        public static async Task<IdentityResultWrapper> ChangeUserPasswordAsync(
            this UserManager<User> mgr, User user, string oldPwd, string newPwd)
        {
            var res = await mgr.ChangePasswordAsync(user, oldPwd, newPwd);
            return new IdentityResultWrapper(res);
        }

        public static async Task<IdentityResultWrapper> SetLockoutAsync(
    this UserManager<User> mgr, User user, bool enable, DateTimeOffset until)
        {
            // 1. Đảm bảo lockout được kích hoạt (chỉ cần 1 lần)
            if (!await mgr.GetLockoutEnabledAsync(user))
            {
                await mgr.SetLockoutEnabledAsync(user, true);
            }

            // 2. Đặt thời điểm kết thúc lock
            var res = await mgr.SetLockoutEndDateAsync(user, until);
            return new IdentityResultWrapper(res);
        }

        public static Task UpdateSecurityStampAsync(
            this UserManager<User> mgr, User user)
            => mgr.UpdateSecurityStampAsync(user);

        public static async Task UpdateRolesAsync(
    this SchoolHealthManagerDbContext context,
    UserManager<User> userMgr,
    User user,
    IEnumerable<string> roleNames,
    CancellationToken ct = default)
        {
            // 1. Make sure the roles exist
            var normalizedRoles = roleNames.Select(r => r.ToUpper()).ToList();
            var existingRoles = await context.Roles
                .Where(r => normalizedRoles.Contains(r.NormalizedName))
                .ToListAsync(ct);

            if (existingRoles.Count != normalizedRoles.Count)
            {
                var missing = normalizedRoles
                    .Except(existingRoles.Select(r => r.NormalizedName));
                throw new InvalidOperationException($"Role(s) not found: {string.Join(',', missing)}");
            }

            // 2. Remove old links
            var oldLinks = context.UserRoles.Where(ur => ur.UserId == user.Id);
            context.UserRoles.RemoveRange(oldLinks);

            // 3. Add new links
            foreach (var role in existingRoles)
            {
                context.UserRoles.Add(new IdentityUserRole<Guid>
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }

            // 4. Let EF save everything inside the same transaction
        }
       
    }
}