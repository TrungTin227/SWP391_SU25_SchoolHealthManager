namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<PagedList<UserDetailsDTO>> GetUserDetailsAsync(int pageNumber, int pageSize);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> GetUserDetailsByIdAsync(Guid id);
        Task<bool> ExistsByUsernameAsync(string username);
    }
}
