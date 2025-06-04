using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Services.Implementations
{
    public class TokenService : ITokenService
    {
        private static readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly SymmetricSecurityKey _secretKey;
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<User> _userManager;

        public TokenService(IOptions<JwtSettings> jwtOptions, UserManager<User> userManager)
        {
            _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            if (string.IsNullOrEmpty(_jwtSettings.Key))
                throw new InvalidOperationException("JWT secret key is not configured.");

            _secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public RefreshTokenInfo GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshTokenInfo
            {
                Token = Convert.ToBase64String(randomNumber),
                Expiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
            };
        }

        private async Task<List<Claim>> GetClaimsAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty),
                new Claim("Gender", user.Gender ?? string.Empty),
                new Claim("securityStamp", await _userManager.GetSecurityStampAsync(user))
            };

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            return new JwtSecurityToken(
                issuer: _jwtSettings.ValidIssuer,
                audience: _jwtSettings.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Expires),
                signingCredentials: signingCredentials
            );
        }

        public async Task<ApiResult<string>> GenerateToken(User user)
        {
            // Kiểm tra tham số đầu vào: nếu user bằng null, trả về Error với thông báo rõ ràng
            if (user == null)
            {
                return ApiResult<string>.Error(
                    data: null,
                    error: new ArgumentNullException(nameof(user), "User is null.")
                );
            }

            try
            {
                // Tạo SigningCredentials từ secret key và thuật toán HmacSha256
                var signingCredentials = new SigningCredentials(_secretKey, SecurityAlgorithms.HmacSha256);

                // Lấy danh sách claims của user (bao gồm username, email, roles, v.v.)
                var claims = await GetClaimsAsync(user).ConfigureAwait(false);

                // Sinh JwtSecurityToken dựa trên signingCredentials và danh sách claims
                var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

                // Chuyển JwtSecurityToken thành chuỗi JWT
                var token = _tokenHandler.WriteToken(tokenOptions);

                // Trả về kết quả thành công: bao gồm token và thông báo
                return ApiResult<string>.Success(
                    data: token,
                    message: "Token generated successfully."
                );
            }
            catch (Exception ex)
            {
                // Nếu xảy ra ngoại lệ trong quá trình tạo token, trả về Failure kèm exception
                return ApiResult<string>.Failure(ex);
            }
        }

    }
}
