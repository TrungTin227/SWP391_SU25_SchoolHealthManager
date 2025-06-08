using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentDTOs.Request;
using Microsoft.AspNetCore.Identity;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class ParentService : IParentService
    {
        private readonly IParentRepository _parentRepository;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public ParentService(IParentRepository parentRepository, UserManager<User> userManager, ICurrentUserService currentUserService, IUserService userService)
        {
            _parentRepository = parentRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _userService = userService;
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                var existing = await _parentRepository.FindByEmailAsync(user.Email);
                if (existing != null)
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
                    await _userManager.AddToRoleAsync(newUser, "Parent");
                    await _userService.SendWelcomeEmailsAsync(newUser.Email);
                }

                return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "Đăng kí user thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<AddParentRequestDTO>> CreateParentAsync(AddParentRequestDTO request)
        {
            try
            {
                if (request == null || request.Id == Guid.Empty)
                {
                    return ApiResult<AddParentRequestDTO>.Failure(new Exception("Yêu cầu nhập User ID!!"));
                }

                if (await _userManager.FindByIdAsync(request.Id.ToString()) == null)
                {
                    return ApiResult<AddParentRequestDTO>.Failure(new Exception("Không tìm thấy User"));
                }

                if (await _parentRepository.GetParentByUserIdAsync(request.Id) != null)
                {
                    return ApiResult<AddParentRequestDTO>.Failure(new Exception("User này đã đăng kí phụ huynh!!"));
                }

                var parent = new Parent
                {
                    UserId = request.Id,
                    Relationship = Relationship.Other,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = _currentUserService.GetUserId() ?? SystemGuid,
                    UpdatedBy = _currentUserService.GetUserId() ?? SystemGuid

                };
                var result = _parentRepository.CreateParentAsync(parent);
                if (result == null)
                {
                    return ApiResult<AddParentRequestDTO>.Failure(new Exception("Gặp lỗi khi tạo phụ huynh!!"));
                }
                return ApiResult<AddParentRequestDTO>.Success(request, "Tạo phụ huynh thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<AddParentRequestDTO>.Failure(ex);
            }
        }
    }
}
