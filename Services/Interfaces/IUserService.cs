namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResult<UserResponse>> RegisterAsync(UserRegisterRequest request);
        Task<ApiResult<UserResponse>> AdminRegisterAsync(AdminCreateUserRequest request);

        Task<ApiResult<CurrentUserResponse>> GetCurrentUserAsync();
        Task<ApiResult<UserResponse>> GetByIdAsync(Guid id);
        Task<ApiResult<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request);
        //Task DeleteAsync(Guid id);
        Task<ApiResult<object>> DeleteUsersAsync(List<Guid> ids);
        Task<ApiResult<RevokeRefreshTokenResponse>> RevokeRefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResult<CurrentUserResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResult<UserResponse>> LoginAsync(UserLoginRequest request);
        Task<ApiResult<PagedList<UserDetailsDTO>>> GetUsersAsync(int pageNumber, int pageSize);
        Task<ApiResult<UserResponse>> UpdateCurrentUserAsync(UpdateUserRequest request);
        Task<ApiResult<string>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ApiResult<UserResponse>> UnlockUserAsync(Guid id);
        Task<ApiResult<UserResponse>> LockUserAsync(Guid id);
        Task<ApiResult<string>> ConfirmEmailAsync(Guid userId, string token);
        Task<ApiResult<string>> ResendConfirmationEmailAsync(string email);
        Task<ApiResult<string>> InitiatePasswordResetAsync(ForgotPasswordRequestDTO request);
        Task<ApiResult<string>> ResetPasswordAsync(ResetPasswordRequestDTO request);
        Task<ApiResult<string>> Send2FACodeAsync();

    }
}