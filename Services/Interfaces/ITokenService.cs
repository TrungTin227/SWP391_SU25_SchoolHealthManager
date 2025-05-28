namespace Services.Interfaces
{
    public interface ITokenService
    {
        Task<ApiResult<string>> GenerateToken(User user);
        string GenerateRefreshToken();
    }
}
