using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces.Services.Commons.User;
using System.Text;

namespace Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;
        private readonly IUserEmailService _userEmailService;
        private readonly string _confirmEmailUri;
        private readonly string _resetPasswordUri;
        private readonly IUserRepository _userRepository;

        public UserService(
            UserManager<User> userManager,
            ITokenService tokenService,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ILogger<UserService> logger,
            IUserEmailService userEmailService,
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userEmailService = userEmailService ?? throw new ArgumentNullException(nameof(userEmailService));
            _confirmEmailUri = configuration["Frontend:ConfirmEmailUri"] ?? throw new ArgumentNullException(nameof(configuration), "ConfirmEmailUri is missing");
            _resetPasswordUri = configuration["Frontend:ResetPasswordUri"] ?? throw new ArgumentNullException(nameof(configuration), "ResetPasswordUri is missing");
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        //public async Task<ApiResult<UserResponse>> RegisterAsync(UserRegisterRequest req)
        //{
        //    if (req == null || string.IsNullOrWhiteSpace(req.Email))
        //        return ApiResult<UserResponse>.Failure(new ArgumentException("Invalid request"));

        //    if (await _userRepository.ExistsByEmailAsync(req.Email))
        //        return ApiResult<UserResponse>.Failure(new InvalidOperationException("Email already in use"));

        //    var result = await _unitOfWork.ExecuteTransactionAsync(async () =>
        //    {
        //        var user = UserMappings.ToDomainUser(req);
        //        var createRes = await _userManager.CreateUserAsync(user, req.Password);
        //        if (!createRes.Succeeded)
        //            return ApiResult<UserResponse>.Failure(new InvalidOperationException(createRes.ErrorMessage));

        //        await _userManager.AddDefaultRoleAsync(user);
        //        return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager), "User registered successfully");
        //    });

        //    if (result.IsSuccess)
        //    {
        //        try
        //        {
        //            await SendWelcomeEmailsAsync(req.Email);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Failed to send welcome emails for {Email}", req.Email);
        //        }
        //    }
        //    return result;
        //}

        public async Task SendWelcomeEmailsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Không cần encode token ở đây, để UserEmailService xử lý
            await Task.WhenAll(
                _userEmailService.SendWelcomeEmailAsync(email),
                _userEmailService.SendEmailConfirmationAsync(email, user.Id, token, _confirmEmailUri)
            );
        }

        public async Task<ApiResult<string>> ConfirmEmailAsync(Guid userId, string encodedToken)
        {
            if (string.IsNullOrWhiteSpace(encodedToken))
                return ApiResult<string>.Failure(new ArgumentException("Invalid token"));

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResult<string>.Failure(new InvalidOperationException("User not found"));

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode token for user {UserId}", userId);
                return ApiResult<string>.Failure(new ArgumentException("Invalid token format"));
            }

            var res = await _userManager.ConfirmEmailAsync(user, token);
            if (!res.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed for user {UserId}. Errors: {Errors}",
                    userId, string.Join(", ", res.Errors.Select(e => e.Description)));
                return ApiResult<string>.Failure(new InvalidOperationException("Email confirmation failed: " + string.Join(", ", res.Errors.Select(e => e.Description))));
            }

            return ApiResult<string>.Success("Email confirmed successfully", "Email confirmed successfully");
        }

        public async Task<ApiResult<string>> ResendConfirmationEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ApiResult<string>.Failure(new ArgumentException("Invalid email"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ApiResult<string>.Success("If the email is valid and unconfirmed, a confirmation email will be sent", "Confirmation email process completed");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return ApiResult<string>.Success("Email is already confirmed", "Email confirmation status checked");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Truyền token nguyên bản, để UserEmailService xử lý việc encode
            await _userEmailService.SendEmailConfirmationAsync(email, user.Id, token, _confirmEmailUri);
            return ApiResult<string>.Success("Confirmation email resent", "Confirmation email sent successfully");
        }

        public async Task<ApiResult<string>> InitiatePasswordResetAsync(ForgotPasswordRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return ApiResult<string>.Failure(new ArgumentException("Invalid email"));

            var genericResponse = ApiResult<string>.Success("If the email is valid, you'll receive password reset instructions", "Password reset process initiated");
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return genericResponse;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            _logger.LogDebug("Raw token: {Token}", token);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            _logger.LogDebug("Encoded token: {encodedToken}", encodedToken);

            await _userEmailService.SendPasswordResetEmailAsync(request.Email, encodedToken, _resetPasswordUri);
            return genericResponse;
        }

        public async Task<ApiResult<string>> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return ApiResult<string>.Failure(new ArgumentException("Invalid request"));

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ApiResult<string>.Failure(new InvalidOperationException("Invalid request"));
            _logger.LogDebug("Incoming token (request): {RequestToken}", request.Token);

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
                _logger.LogDebug("Decoded token: {Decoded}", token);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi decode token");
                return ApiResult<string>.Failure(new ArgumentException("Invalid token"));
            }

            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!result.Succeeded)
                return ApiResult<string>.Failure(new InvalidOperationException("Password reset failed. Please try again"));

            await _userEmailService.SendPasswordChangedNotificationAsync(request.Email);
            return ApiResult<string>.Success("Password reset successfully", "Password has been reset successfully");
        }

        public async Task<ApiResult<string>> Send2FACodeAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                return ApiResult<string>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return ApiResult<string>.Failure(new InvalidOperationException("User not found"));

            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
            await _userEmailService.Send2FACodeAsync(user.Email, code);
            return ApiResult<string>.Success("2FA code sent", "Two-factor authentication code sent successfully");
        }

        //public async Task<ApiResult<UserResponse>> AdminRegisterAsync(AdminCreateUserRequest req)
        //{
        //    if (req == null || string.IsNullOrWhiteSpace(req.Email))
        //        return ApiResult<UserResponse>.Failure(new ArgumentException("Invalid request"));

        //    if (!_currentUserService.IsAdmin())
        //        return ApiResult<UserResponse>.Failure(new UnauthorizedAccessException("Forbidden: Only admins can register users"));

        //    if (await _userRepository.ExistsByEmailAsync(req.Email))
        //        return ApiResult<UserResponse>.Failure(new InvalidOperationException("Email already in use"));

        //    _logger.LogInformation("Admin registering user: {Email}", req.Email);

        //    return await _unitOfWork.ExecuteTransactionAsync(async () =>
        //    {
        //        var user = UserMappings.ToDomainUser(req);
        //        var create = await _userManager.CreateUserAsync(user, req.Password);
        //        if (!create.Succeeded)
        //            return ApiResult<UserResponse>.Failure(new InvalidOperationException(create.ErrorMessage));

        //        await _userManager.AddRolesAsync(user, req.Roles);
        //        var token = await _tokenService.GenerateToken(user);
        //        return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager, token.Data), "User created successfully by admin");
        //    });
        //}

        public async Task<ApiResult<UserResponse>> LoginAsync(UserLoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return ApiResult<UserResponse>.Failure(new ArgumentException("Invalid request"));

            _logger.LogInformation("Login attempt: {Email}", req.Email);

            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, req.Password))
            {
                if (user != null) await _userManager.AccessFailedAsync(user);
                return ApiResult<UserResponse>.Failure(new UnauthorizedAccessException("Invalid email or password"));
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("Please confirm your email before logging in"));

            if (await _userManager.IsLockedOutAsync(user))
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("Account is locked"));

            await _userManager.ResetAccessFailedAsync(user);

            var token = await _tokenService.GenerateToken(user);
            var refreshTokenInfo = _tokenService.GenerateRefreshToken();
            await _userManager.SetRefreshTokenAsync(user, refreshTokenInfo);

            return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager, token.Data, refreshTokenInfo.Token), "Login successful");
        }

        public async Task<ApiResult<UserResponse>> GetByIdAsync(Guid id)
        {
            var userDetails = await _userRepository.GetUserDetailsByIdAsync(id);
            if (userDetails == null)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("User not found"));

            return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(userDetails, _userManager), "User retrieved successfully");
        }

        public async Task<ApiResult<CurrentUserResponse>> GetCurrentUserAsync()
        {
            var uid = _currentUserService.GetUserId();
            if (uid == null)
                return ApiResult<CurrentUserResponse>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user == null)
                return ApiResult<CurrentUserResponse>.Failure(new InvalidOperationException("User not found"));

            var token = await _tokenService.GenerateToken(user);
            return ApiResult<CurrentUserResponse>.Success(UserMappings.ToCurrentUserResponse(user, token.Data), "Current user retrieved successfully");
        }

        public async Task<ApiResult<CurrentUserResponse>> RefreshTokenAsync(RefreshTokenRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.RefreshToken))
                return ApiResult<CurrentUserResponse>.Failure(new ArgumentException("Invalid request"));

            var uid = _currentUserService.GetUserId();
            if (uid == null)
                return ApiResult<CurrentUserResponse>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user == null || !await _userManager.ValidateRefreshTokenAsync(user, req.RefreshToken))
                return ApiResult<CurrentUserResponse>.Failure(new UnauthorizedAccessException("Invalid refresh token"));

            var token = await _tokenService.GenerateToken(user);
            return ApiResult<CurrentUserResponse>.Success(UserMappings.ToCurrentUserResponse(user, token.Data), "Token refreshed successfully");
        }

        public async Task<ApiResult<RevokeRefreshTokenResponse>> RevokeRefreshTokenAsync(RefreshTokenRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.RefreshToken))
                return ApiResult<RevokeRefreshTokenResponse>.Failure(new ArgumentException("Invalid request"));

            var uid = _currentUserService.GetUserId();
            if (uid == null)
                return ApiResult<RevokeRefreshTokenResponse>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user == null || !await _userManager.ValidateRefreshTokenAsync(user, req.RefreshToken))
                return ApiResult<RevokeRefreshTokenResponse>.Failure(new UnauthorizedAccessException("Invalid refresh token"));

            var rem = await _userManager.RemoveRefreshTokenAsync(user);
            if (!rem.Succeeded)
                return ApiResult<RevokeRefreshTokenResponse>.Failure(new InvalidOperationException(rem.ErrorMessage));

            return ApiResult<RevokeRefreshTokenResponse>.Success(new RevokeRefreshTokenResponse { Message = "Revoked" }, "Refresh token revoked successfully");
        }

        public async Task<ApiResult<string>> ChangePasswordAsync(ChangePasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.OldPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
                return ApiResult<string>.Failure(new ArgumentException("Invalid request"));

            var uid = _currentUserService.GetUserId();
            if (uid == null)
                return ApiResult<string>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user == null)
                return ApiResult<string>.Failure(new InvalidOperationException("User not found"));

            var res = await _userManager.ChangeUserPasswordAsync(user, req.OldPassword, req.NewPassword);
            if (!res.Succeeded)
                return ApiResult<string>.Failure(new InvalidOperationException(res.ErrorMessage));

            await _userManager.UpdateSecurityStampAsync(user);
            await _userEmailService.SendPasswordChangedNotificationAsync(user.Email);
            return ApiResult<string>.Success("Password changed successfully", "Password changed successfully");
        }

        public async Task<ApiResult<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest req)
        {
            if (req == null)
                return ApiResult<UserResponse>.Failure(new ArgumentException("Invalid request"));

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("User not found"));

            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                UserMappings.ApplyUpdate(req, user);
                user.UpdatedAt = DateTime.UtcNow;

                if (req.Roles?.Any() == true && _currentUserService.IsAdmin())
                    await _userManager.UpdateRolesAsync(user, req.Roles);

                var upd = await _userManager.UpdateAsync(user);
                if (!upd.Succeeded)
                    return ApiResult<UserResponse>.Failure(new InvalidOperationException(string.Join(", ", upd.Errors.Select(e => e.Description))));

                return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager), "User updated successfully");
            });
        }

        public async Task<ApiResult<UserResponse>> UpdateCurrentUserAsync(UpdateUserRequest req)
        {
            if (req == null)
                return ApiResult<UserResponse>.Failure(new ArgumentException("Invalid request"));

            var uid = _currentUserService.GetUserId();
            if (uid == null)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("User not found"));

            var user = await _userManager.FindByIdAsync(uid.ToString());
            if (user == null)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("User not found"));

            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                UserMappings.ApplyUpdate(req, user);
                user.UpdatedAt = DateTime.UtcNow;

                var upd = await _userManager.UpdateAsync(user);
                if (!upd.Succeeded)
                    return ApiResult<UserResponse>.Failure(new InvalidOperationException(string.Join(", ", upd.Errors.Select(e => e.Description))));

                return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager), "Current user updated successfully");
            });
        }

        public Task<ApiResult<UserResponse>> LockUserAsync(Guid id) =>
            ChangeLockoutAsync(id, true, DateTimeOffset.MaxValue);

        public Task<ApiResult<UserResponse>> UnlockUserAsync(Guid id) =>
            ChangeLockoutAsync(id, false, DateTimeOffset.UtcNow);

        private async Task<ApiResult<UserResponse>> ChangeLockoutAsync(Guid id, bool enable, DateTimeOffset until)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException("User not found"));

            var res = await _userManager.SetLockoutAsync(user, enable, until);
            if (!res.Succeeded)
                return ApiResult<UserResponse>.Failure(new InvalidOperationException(res.ErrorMessage));

            string message = enable ? "User locked successfully" : "User unlocked successfully";
            return ApiResult<UserResponse>.Success(await UserMappings.ToUserResponseAsync(user, _userManager), message);
        }

        public async Task<ApiResult<object>> DeleteUsersAsync(List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return ApiResult<object>.Success(null, "No users to delete");

            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                foreach (var id in ids)
                {
                    var user = await _userManager.FindByIdAsync(id.ToString());
                    if (user == null)
                        return ApiResult<object>.Failure(new InvalidOperationException($"User {id} not found"));

                    var del = await _userManager.DeleteAsync(user);
                    if (!del.Succeeded)
                        return ApiResult<object>.Failure(new InvalidOperationException(string.Join(", ", del.Errors.Select(e => e.Description))));
                }
                return ApiResult<object>.Success(null, $"Successfully deleted {ids.Count} user(s)");
            });
        }

        public async Task<ApiResult<PagedList<UserDetailsDTO>>> GetUsersAsync(int page, int size)
        {
            if (page < 1 || size < 1)
                return ApiResult<PagedList<UserDetailsDTO>>.Failure(new ArgumentException("Invalid pagination parameters"));

            var list = await _userRepository.GetUserDetailsAsync(page, size);
            return ApiResult<PagedList<UserDetailsDTO>>.Success(list, $"Retrieved {list.Count} users from page {page}");
        }

        
    }
}