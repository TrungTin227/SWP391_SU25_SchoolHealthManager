namespace Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User, Guid>   
    {
        Task<PagedList<UserDetailsDTO>> GetUserDetailsAsync(int pageNumber, int pageSize);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> GetUserDetailsByIdAsync(Guid id);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsAsync(Guid userId);
        Task UpdateRolesAsync(User user, IEnumerable<string> roleNames);

        Task<PagedList<User>> SearchUsersAsync(
    string? searchTerm,
    RoleType? role,
    int page,
    int size);
    }
}
