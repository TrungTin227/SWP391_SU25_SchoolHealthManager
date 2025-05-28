using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SchoolHealthManagerDbContext _context;

        public UserRepository(SchoolHealthManagerDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<UserDetailsDTO>> GetUserDetailsAsync(int pageNumber, int pageSize)
        {
            // Query optimization: Count and retrieve users in a single database round trip
            var query = _context.Users.AsNoTracking();

            var totalCount = await query.CountAsync();

            // Prepare efficient query for paged results
            var pagedUsers = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    User = u,
                    Id = u.Id
                })
                .ToListAsync();

            var userIds = pagedUsers.Select(x => x.Id).ToList();

            // Fetch all roles for these users in a single query with projection
            var userRolesDict = await _context.Set<IdentityUserRole<Guid>>()
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(
                    _context.Roles.AsNoTracking(),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { UserId = ur.UserId, RoleName = r.Name }
                )
                .GroupBy(x => x.UserId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Select(x => x.RoleName).ToList()
                );

            // Map to DTOs with optimized lookups
            var userDetailsList = pagedUsers.Select(u => new UserDetailsDTO
            {
                Id = u.User.Id,
                FirstName = u.User.FirstName ?? string.Empty,
                LastName = u.User.LastName ?? string.Empty,
                Email = u.User.Email ?? string.Empty,
                Gender = u.User.Gender.ToString(), 
                CreateAt = u.User.CreatedAt,
                UpdateAt = u.User.UpdatedAt,
                Roles = userRolesDict.TryGetValue(u.User.Id, out var roles) ? roles : new List<string>()
            }).ToList();

            return new PagedList<UserDetailsDTO>(userDetailsList, totalCount, pageNumber, pageSize);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User> GetUserDetailsByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserName == username);
        }
    }
}