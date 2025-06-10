using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<ParentService> _logger;

        public ParentService(IParentRepository parentRepository, UserManager<User> userManager, ICurrentUserService currentUserService, IUserService userService, ILogger<ParentService> logger)
        {
            _parentRepository = parentRepository;
            _userManager = userManager;
            _currentUserService = currentUserService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                var existing = await _parentRepository.FindByEmailAsync(user.Email);
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

        public async Task<ApiResult<List<GetAllParentDTO>>> GetAllParentsAsync()
        {
            try
            {
                var parents = await _parentRepository.GetAllParentDtoAsync();
                if (parents == null || !parents.Any())
                {
                    return ApiResult<List<GetAllParentDTO>>.Failure(new Exception("Không tìm thấy phụ huynh nào!!"));
                }
                return ApiResult<List<GetAllParentDTO>>.Success(parents, "Lấy danh sách phụ huynh thành công!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<GetAllParentDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateRelationshipByParentIdAsync(UpdateRelationshipByParentId request)
        {
            try
            {
                if (request == null || request.ParentId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(new Exception("Yêu cầu nhập Parent ID!!"));
                }

                // Nếu Relationship là enum, validate luôn
                if (!Enum.IsDefined(typeof(Relationship), request.Relationship))
                {
                    return ApiResult<bool>.Failure(new Exception("Giá trị mối quan hệ không hợp lệ!"));
                }

                if (await _parentRepository.GetParentByUserIdAsync(request.ParentId) == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy phụ huynh với ID này: " + request.ParentId + " !!"));
                }

                var result = await _parentRepository.UpdateRelationshipByParentIdAsync(request);

                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Cập nhật mối quan hệ thất bại!!"));
                }

                return ApiResult<bool>.Success(true, "Cập nhật mối quan hệ thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật mối quan hệ phụ huynh");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteByParentIdAsync(Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return ApiResult<bool>.Failure(new Exception("Yêu cầu nhập Parent ID!!"));
                }
                var parent = await _parentRepository.GetParentByUserIdAsync(parentId);
                if (parent == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy phụ huynh với ID này: " + parentId + " !!"));
                }
                parent.DeletedAt = DateTime.UtcNow;
                parent.DeletedBy = _currentUserService.GetUserId() ?? SystemGuid;
                var result = await _parentRepository.SoftDeleteByParentId(parentId);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Xóa phụ huynh thất bại!!"));
                }
                return ApiResult<bool>.Success(true, "Xóa phụ huynh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phụ huynh");
                return ApiResult<bool>.Failure(ex);
            }
        }
        public async Task<ApiResult<UserRegisterRespondDTO>> RegisterParentUserAsync(UserRegisterRequestDTO user)
        {
            try
            {
                // Check nếu email đã tồn tại
                var existing = await _parentRepository.FindByEmailAsync(user.Email);
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

                // Gán role "Parent" và gửi mail
                await _userManager.AddToRoleAsync(newUser, "Parent");
                await _userService.SendWelcomeEmailsAsync(newUser.Email);

                // Tạo record Parent luôn
                var parent = new Parent
                {
                    UserId = newUser.Id,
                    Relationship = Relationship.Other,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    UpdatedBy = currentUserId
                };

                var parentResult = await _parentRepository.CreateParentAsync(parent);
                if (parentResult == null)
                {
                    return ApiResult<UserRegisterRespondDTO>.Failure(new Exception("Tạo user thành công nhưng tạo phụ huynh thất bại!!"));
                }

                return ApiResult<UserRegisterRespondDTO>.Success(UserMappings.ToUserRegisterResponse(newUser), "Đăng kí phụ huynh thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<UserRegisterRespondDTO>.Failure(ex);
            }
        }

    }
}
