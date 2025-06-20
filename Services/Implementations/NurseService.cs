using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using DTOs.NurseDTOs.Request;
using DTOs.NurseDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Repositories.Implementations;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class NurseService : INurseService
    {
        private readonly INurseRepository _nurseRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private readonly ILogger<NurseService> _logger;

        public NurseService(INurseRepository nurseRepository, UserManager<User> userManager, ICurrentUserService currentUserService, IUserService userService, ILogger<NurseService> logger)
        {
            _nurseRepository = nurseRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _userService = userService;
            _logger = logger;
        }
        public async Task<ApiResult<AddNurseRequestDTO>> CreateNurseAsync(AddNurseRequestDTO request)
        {
            try
            {
                if (request == null || request.Id == Guid.Empty)
                {
                    return ApiResult<AddNurseRequestDTO>.Failure(new Exception("Yêu cầu nhập User ID!!"));
                }

                if (await _userManager.FindByIdAsync(request.Id.ToString()) == null)
                {
                    return ApiResult<AddNurseRequestDTO>.Failure(new Exception("Không tìm thấy User"));
                }

                if (await _nurseRepository.GetNurseByUserIdAsync(request.Id) != null)
                {
                    return ApiResult<AddNurseRequestDTO>.Failure(new Exception("User này đã đăng kí Nhân Viên Y Tế!!"));
                }

                var nurse = new NurseProfile
                {
                    UserId = request.Id,
                    Position = Position.Other.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.GetUserId() ?? SystemGuid,
                    UpdatedBy = _currentUserService.GetUserId() ?? SystemGuid

                };
                var result = _nurseRepository.CreateNurseAsync(nurse);
                if (result == null)
                {
                    return ApiResult<AddNurseRequestDTO>.Failure(new Exception("Gặp lỗi khi tạo Nhân Viên Y tế!!"));
                }
                return ApiResult<AddNurseRequestDTO>.Success(request, "Tạo Nhân Viên Y Tế thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<AddNurseRequestDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<GetNurseDTO>>> GetAllNursesAsync()
        {
            try
            {
                var nurses = await _nurseRepository.GetNurseDtoAsync();
                if (nurses == null || !nurses.Any())
                {
                    return ApiResult<List<GetNurseDTO>>.Failure(new Exception("Không tìm thấy Nhân Viên Y Tế nào!!"));
                }
                return ApiResult<List<GetNurseDTO>>.Success(nurses, "Lấy danh sách Nhân Viên Y Tế thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetNurseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterNurseUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                var existing = await _nurseRepository.FindByEmailAsync(user.Email);
                if (existing == true)
                {
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Mail đã được sử dụng, vui lòng sử dụng mail khác!!"));
                }
                var currentUserId = _currentUserService.GetUserId() ?? SystemGuid;

                //if (currentUserId == SystemGuid)
                //{
                //    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Current user is not authenticated."));
                //}

                var newUser = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.Email,
                    Gender = user.Gender,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = currentUserId,
                    IsDeleted = false,
                    EmailConfirmed = false  // optional
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception($"Đăng kí thất bại!!! Lỗi: {errors}"));
                }
                else if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, "SchoolNurse");
                    await _userService.SendWelcomeEmailsAsync(newUser.Email);
                }

                return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "Đăng kí user thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                // Check nếu email đã tồn tại
                var existing = await _nurseRepository.FindByEmailAsync(user.Email);
                if (existing)
                {
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Mail đã được sử dụng, vui lòng sử dụng mail khác!!"));
                }

                var currentUserId = _currentUserService.GetUserId() ?? SystemGuid;

                // Tạo User mới
                var newUser = new User
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserName = user.Email,
                    Gender = user.Gender,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = currentUserId,
                    IsDeleted = false,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(newUser, user.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception($"Đăng kí thất bại!!! Lỗi: {errors}"));
                }

                await _userManager.AddToRoleAsync(newUser, "Nurse");
                await _userService.SendWelcomeEmailsAsync(newUser.Email);

             
                var nurse = new NurseProfile
                {
                    UserId = newUser.Id,
                    Position = Position.Other.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    UpdatedBy = currentUserId
                };

                var nurseResult = await _nurseRepository.CreateNurseAsync(nurse);
                if (nurseResult == null)
                {
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Tạo user thành công nhưng tạo Nhân Viên Y Tế thất bại!!"));
                }

                return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "Đăng kí Nhân Viên Y Tế thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteByNurseIdAsync(Guid NurseId)
        { 
            try
            {
                if (NurseId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(new Exception("Yêu cầu nhập Nurse ID!!"));
                }
                var nurse = await _nurseRepository.GetNurseByUserIdAsync(NurseId);
                if (nurse == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy Nhân Viên Y Tế với ID này: " + NurseId + " !!"));
                }
                nurse.DeletedAt = DateTime.UtcNow;
                nurse.DeletedBy = _currentUserService.GetUserId() ?? SystemGuid;
                var result = await _nurseRepository.SoftDeleteByNurseId(NurseId);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Xóa Nhân Viên Y Tế thất bại!!"));
                }
                return ApiResult<bool>.Success(true, "Xóa Nhân Viên Y Tế thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Nhân Viên Y Tế");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public Task<ApiResult<bool>> UpdateNurseAsync(UpdateNurseRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
