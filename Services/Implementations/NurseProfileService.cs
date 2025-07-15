using DTOs.NurseProfile.Request;
using DTOs.NurseProfile.Response;
using DTOs.GlobalDTO.Respond;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class NurseProfileService : BaseService<NurseProfile, Guid>, INurseProfileService
    {
        private readonly INurseProfileRepository _nurseRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<NurseProfileService> _logger;

        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public NurseProfileService(
            IGenericRepository<NurseProfile, Guid> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            INurseProfileRepository nurseRepository,
            UserManager<User> userManager,
            IUserService userService,
            ILogger<NurseProfileService> logger
        ) : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _nurseRepository = nurseRepository;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                var exists = await _nurseRepository.FindByEmailAsync(user.Email);
                if (exists)
                {
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Email đã được sử dụng!"));
                }

                var userId = _currentUserService.GetUserId() ?? SystemGuid;

                var newUser = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.Email,
                    Gender = user.Gender,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userId,
                    IsDeleted = false,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception(errors));
                }

                await _userManager.AddToRoleAsync(newUser, "Nurse");
                await _userService.SendWelcomeEmailsAsync(newUser.Email);

                return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "Tạo tài khoản thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đăng ký tài khoản");
                return ApiResult<UserRegisterRespondDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterNurseUserAsync(UserRegisterRequestDTO user)
        {
            var registerResult = await RegisterUserAsync(user);
            if (!registerResult.IsSuccess || registerResult.Data == null)
            {
                return registerResult;
            }

            var nurse = new NurseProfile
            {
                UserId = registerResult.Data.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.GetUserId() ?? SystemGuid,
                UpdatedBy = _currentUserService.GetUserId() ?? SystemGuid
            };

            var created = await _nurseRepository.CreateNurseAsync(nurse);
            if (created == null)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Tạo hồ sơ y tá thất bại!"));
            }

            return registerResult;
        }

        public async Task<ApiResult<AddNurseRequest>> CreateNurseAsync(AddNurseRequest request)
        {
            try
            {
                var nurse = new NurseProfile
                {
                    UserId = request.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.GetUserId() ?? SystemGuid,
                    UpdatedBy = _currentUserService.GetUserId() ?? SystemGuid
                };

                var result = await _nurseRepository.CreateNurseAsync(nurse);
                if (result == null)
                {
                    return ApiResult<AddNurseRequest>.Failure(new Exception("Tạo hồ sơ y tá thất bại!"));
                }

                return ApiResult<AddNurseRequest>.Success(request, "Tạo hồ sơ y tá thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hồ sơ y tá");
                return ApiResult<AddNurseRequest>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<NurseProfileRespondDTOs>>> GetAllNursesAsync()
        {
            try
            {
                var nurses = await _nurseRepository.GetNurseDtoAsync();
                if (nurses == null || !nurses.Any())
                {
                    return ApiResult<List<NurseProfileRespondDTOs>>.Failure(new Exception("Không có hồ sơ y tá nào!"));
                }

                return ApiResult<List<NurseProfileRespondDTOs>>.Success(nurses, "Lấy danh sách thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách y tá");
                return ApiResult<List<NurseProfileRespondDTOs>>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateNurseAsync(UpdateNurseRequest request)
        {
            try
            {
                var result = await _nurseRepository.UpdateNurseAsync(request);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Cập nhật thất bại!"));
                }

                return ApiResult<bool>.Success(true, "Cập nhật hồ sơ y tá thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật y tá");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteByNurseIdAsync(Guid nurseId)
        {
            try
            {
                var result = await _nurseRepository.SoftDeleteByNurseId(nurseId);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Xóa mềm thất bại!"));
                }

                return ApiResult<bool>.Success(true, "Xóa mềm thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xóa mềm y tá");
                return ApiResult<bool>.Failure(ex);
            }
        }
    }
}
