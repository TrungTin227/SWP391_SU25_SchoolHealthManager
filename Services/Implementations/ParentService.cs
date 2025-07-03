using DTOs.GlobalDTO.Respond;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ParentService : BaseService<Parent,Guid>, IParentService
    {
        private readonly IParentRepository _parentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly ILogger<ParentService> _logger;

        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public ParentService(
            IGenericRepository<Parent, Guid> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            IParentRepository parentRepository,
            UserManager<User> userManager,
            IUserService userService,
            ILogger<ParentService> logger
        ) : base(repository, currentUserService, unitOfWork, currentTime)
        {
            _parentRepository = parentRepository;
            _userManager = userManager;
            _userService = userService;
            _logger = logger;
        }
        #region get parent
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

        #endregion
        #region Create, Update relationship Parent
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
        #endregion
        #region delete and restore parent

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

        public async Task<ApiResult<List<RestoreResponseDTO>>> SoftDeleteByParentIdListAsync(List<Guid> parentIds)
        {
            try
            {
                var responseList = new List<RestoreResponseDTO>();

                foreach (var parentId in parentIds)
                {
                    try
                    {
                        if (parentId == Guid.Empty)
                        {
                            responseList.Add(new RestoreResponseDTO
                            {
                                Id = parentId,
                                IsSuccess = false,
                                Message = "Parent ID không hợp lệ!!"
                            });
                            continue;
                        }

                        var parent = await _parentRepository.GetParentByUserIdAsync(parentId);
                        if (parent == null)
                        {
                            responseList.Add(new RestoreResponseDTO
                            {
                                Id = parentId,
                                IsSuccess = false,
                                Message = $"Không tìm thấy phụ huynh với ID: {parentId}"
                            });
                            continue;
                        }

                        parent.DeletedAt = DateTime.UtcNow;
                        parent.DeletedBy = _currentUserService.GetUserId() ?? SystemGuid;

                        var result = await _parentRepository.SoftDeleteByParentId(parentId);
                        if (!result)
                        {
                            responseList.Add(new RestoreResponseDTO
                            {
                                Id = parentId,
                                IsSuccess = false,
                                Message = "Xóa phụ huynh thất bại!!"
                            });
                            continue;
                        }

                        responseList.Add(new RestoreResponseDTO
                        {
                            Id = parentId,
                            IsSuccess = true,
                            Message = "Xóa phụ huynh thành công!!"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi khi xóa phụ huynh ID: {ParentId}", parentId);
                        responseList.Add(new RestoreResponseDTO
                        {
                            Id = parentId,
                            IsSuccess = false,
                            Message = "Lỗi hệ thống: " + ex.Message
                        });
                    }
                }
                return ApiResult<List<RestoreResponseDTO>>.Success(responseList, "Xử lý xóa danh sách phụ huynh hoàn tất");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa danh sách phụ huynh");
                return ApiResult<List<RestoreResponseDTO>>.Failure(ex);
            }
        }


        public async Task<RestoreResponseDTO> RestoreParentAsync(Guid id, Guid? userId)
        {
            try
            {
                var restored = await _repository.RestoreAsync(id, userId);
                return new RestoreResponseDTO
                {
                    Id = id,
                    IsSuccess = restored,
                    Message = restored ? "Khôi phục phụ huynh thành công" : "Không tìm thấy hoặc không thể khôi phục phụ huynh"
                };
            }
            catch (Exception ex)
            {
                return new RestoreResponseDTO { Id = id, IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<RestoreResponseDTO>> RestoreParentRangeAsync(List<Guid> ids, Guid? userId)
        {
            var results = new List<RestoreResponseDTO>();
            foreach (var id in ids)
            {
                results.Add(await RestoreParentAsync(id, userId));
            }
            return results;
        }
        #endregion
    }
}
