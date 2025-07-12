using BusinessObjects;
using DTOs.GlobalDTO.Respond;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;
using DTOs.StudentDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class HealProfileService : BaseService<HealthProfile,Guid>, IHealProfileService
    {
        private readonly ILogger<HealProfileService> _logger;
        public HealProfileService( ILogger<HealProfileService> logger,
            IGenericRepository<HealthProfile,
            Guid> repository,
            ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime
            ) : 
            base(repository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger;
        }
        #region Create, Update HealProfile
        public async Task<ApiResult<HealProfileResponseDTO>> CreateHealProfileAsync(CreateHealProfileRequestDTO request)
        {
            try
            {
                var now = _currentTime.GetVietnamTime();
                var student = await _unitOfWork.StudentRepository
                   .GetQueryable()
                   .FirstOrDefaultAsync(s => s.StudentCode == request.StudentCode);
                var parent = await _unitOfWork.ParentRepository.GetParentByUserIdAsync(student.ParentUserId);

                if (student == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Không tìm thấy học sinh!!"));
                }


                // ✅ Query version max hiện tại
                int maxVersion = await _repository
                    .GetQueryable()
                    .Where(hp => hp.StudentId == student.Id)
                    .MaxAsync(hp => (int?)hp.Version) ?? 0;

                // ✅ Convert DTO sang entity và gán Version
                HealthProfile healthProfile = HealProfileMappings.ToEntity(request, student, parent);
                healthProfile.Version = maxVersion + 1;
                healthProfile.ProfileDate = now;

                await _repository.AddAsync(healthProfile);
                await _unitOfWork.SaveChangesAsync();

                GetAllStudentDTO studentdto = StudentMappings.ToGetAllStudent(student);
                var response = HealProfileMappings.FromEntity(healthProfile);
                response.StudentInformation = studentdto;

                return ApiResult<HealProfileResponseDTO>.Success(response, "Tạo hồ sơ sức khỏe thành công!!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hồ sơ sức khỏe");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi tạo hồ sơ sức khỏe: " + ex.Message));
            }
        }

        public async Task<ApiResult<HealProfileResponseDTO>> UpdateHealProfileByStudentCodeAsync(string studentcode, UpdateHealProfileRequestDTO request)
        {
            try
            {
                // Tìm student theo studentcode
                var now = _currentTime.GetVietnamTime();
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StudentCode == studentcode);

                if (student == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Không tìm thấy học sinh với mã được cung cấp."));
                }

                // Tìm HealProfile mới nhất theo StudentId
                var entity = await _unitOfWork.HealProfileRepository
                    .GetQueryable()
                    .Where(h => h.StudentId == student.Id)
                    .OrderByDescending(h => h.Version)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Không tìm thấy hồ sơ sức khỏe."));
                }

                entity.ProfileDate = now;

                if (request.Allergies != null)
                    entity.Allergies = request.Allergies;

                if (request.ChronicConditions != null)
                    entity.ChronicConditions = request.ChronicConditions;

                if (request.TreatmentHistory != null)
                    entity.TreatmentHistory = request.TreatmentHistory;

                if (request.VaccinationSummary != null)
                    entity.VaccinationSummary = request.VaccinationSummary;

                if (request.Vision.HasValue)
                    entity.Vision = request.Vision.Value;

                if (request.Hearing.HasValue)
                    entity.Hearing = request.Hearing.Value;

                // Audit info nếu hàm dc mở
                // SetAuditFieldsForUpdate(entity);

                await _unitOfWork.SaveChangesAsync();

                // Map sang ResponseDTO với string enums
                var response = HealProfileMappings.FromEntity(entity);

                return ApiResult<HealProfileResponseDTO>.Success(response, "Cập nhật thông tin hồ sơ sức khỏe thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hồ sơ sức khỏe");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi cập nhật hồ sơ sức khỏe: " + ex.Message));
            }
        }
        #endregion
        #region Get HealProfile
        public async Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfileByStudentCodeAsync(string studentcode)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StudentCode == studentcode);
                if (student == null)
                {
                    return ApiResult<List<HealProfileResponseDTO>>.Failure(
                        new Exception("Không tìm thấy học sinh với mã học sinh này."));
                }

                var profiles = await _unitOfWork.HealProfileRepository
                    .GetAllAsync(p => p.StudentId == student.Id);

                if (profiles == null || !profiles.Any())
                {
                    return ApiResult<List<HealProfileResponseDTO>>.Failure(
                        new Exception("Không tìm thấy hồ sơ sức khỏe nào cho học sinh này."));
                }

                var result = profiles
                    .Select(HealProfileMappings.FromEntity) // map từng item
                    .ToList();

                return ApiResult<List<HealProfileResponseDTO>>.Success(result, "Lấy danh sách hồ sơ sức khỏe thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hồ sơ sức khỏe theo mã học sinh");
                return ApiResult<List<HealProfileResponseDTO>>.Failure(new Exception("Lỗi khi lấy danh sách hồ sơ sức khỏe: " + ex.Message));
            }
        }


        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByIdAsync(Guid id)
        {
            try
            {
                var entity = await _unitOfWork.HealProfileRepository.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Không tìm thấy hồ sơ sức khỏe."));
                }

                // Map sang ResponseDTO với string enums
                var response = HealProfileMappings.FromEntity(entity);
                return ApiResult<HealProfileResponseDTO>.Success(response, "Lấy thông tin hồ sơ sức khỏe thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ sức khỏe theo ID");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi lấy hồ sơ sức khỏe: " + ex.Message));
            }
        }

        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByParentIdAsync(Guid parentId)
        {
            try
            {
                var profiles = await _unitOfWork.HealProfileRepository
                    .GetAllAsync(p => p.ParentId == parentId);
                if (profiles == null || !profiles.Any())
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(
                        new Exception("Không tìm thấy hồ sơ sức khỏe nào dựa trên id phụ huynh này."));
                }
                // Giả sử chỉ lấy hồ sơ đầu tiên nếu có nhiều
                var profile = profiles.First();
                var response = HealProfileMappings.FromEntity(profile);
                return ApiResult<HealProfileResponseDTO>.Success(response, "Lấy thông tin hồ sơ sức khỏe thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ sức khỏe theo ID phụ huynh");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi lấy hồ sơ sức khỏe theo id của phụ huynh: " + ex.Message));
            }
        }

        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByStudentCodeAsync(string studentcode)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository
                   .GetQueryable()
                   .FirstOrDefaultAsync(s => s.StudentCode == studentcode);
                var profiles = await _unitOfWork.HealProfileRepository
                               .GetAllAsync(p => p.StudentId == student.Id);
                if (profiles == null || !profiles.Any())
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(
                        new Exception("Không tìm thấy hồ sơ sức khỏe nào dựa trên mã học sinh này."));
                }
                // Giả sử chỉ lấy hồ sơ đầu tiên nếu có nhiều
                var profile = profiles.First();
                var response = HealProfileMappings.FromEntity(profile);
                return ApiResult<HealProfileResponseDTO>.Success(response, "Lấy thông tin hồ sơ sức khỏe thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ sức khỏe theo mã học sinh");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi lấy hồ sơ sức khỏe theo mã học sinh: " + ex.Message));
            }
        }

        public async Task<ApiResult<HealProfileResponseDTO>> GetNewestHealProfileByStudentCodeAsync(string studentcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentcode))
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(
                        new Exception("Mã học sinh không được null hoặc rỗng."));
                }
                // Tìm student theo studentcode
                var student = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.StudentCode == studentcode);

                if (student == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(
                        new Exception("Không tìm thấy học sinh với mã được cung cấp."));
                }

                // Tìm hồ sơ sức khỏe mới nhất theo StudentId (version cao nhất)
                var profile = await _unitOfWork.HealProfileRepository
                    .GetQueryable()
                    .Where(p => p.StudentId == student.Id)
                    .OrderByDescending(p => p.Version)
                    .FirstOrDefaultAsync();

                if (profile == null)
                {
                    return ApiResult<HealProfileResponseDTO>.Failure(
                        new Exception("Không tìm thấy hồ sơ sức khỏe nào dựa trên mã học sinh này."));
                }

                // Map sang ResponseDTO
                var response = HealProfileMappings.FromEntity(profile);
                response.StudentInformation = StudentMappings.ToGetAllStudent(student);
                return ApiResult<HealProfileResponseDTO>.Success(response, "Lấy thông tin hồ sơ sức khỏe thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hồ sơ sức khỏe mới nhất theo mã học sinh");
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Lỗi khi lấy hồ sơ sức khỏe mới nhất theo mã học sinh: " + ex.Message));
            }
        }
        #endregion
        #region SoftDelete and Restore
        public async Task<ApiResult<RestoreResponseDTO>> SoftDeleteHealthProfileAsync(Guid id)
        {
            try
            {
                var responseList = await SoftDeleteHealthProfilesAsync(new List<Guid> { id });
                var response = responseList.Data?.FirstOrDefault();

                if (response == null)
                    return ApiResult<RestoreResponseDTO>.Failure(new Exception("Không nhận được kết quả xóa"));

                if (!response.IsSuccess)
                    return ApiResult<RestoreResponseDTO>.Failure(new Exception(response.Message));

                return ApiResult<RestoreResponseDTO>.Success(response, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hồ sơ sức khỏe ID: {HealthProfileId}", id);
                return ApiResult<RestoreResponseDTO>.Failure(new Exception("Lỗi hệ thống khi xóa mềm hồ sơ sức khỏe theo id: " + ex.Message));
            }
        }


        public async Task<ApiResult<List<RestoreResponseDTO>>> SoftDeleteHealthProfilesAsync(List<Guid> ids)
        {
            var responseList = new List<RestoreResponseDTO>();
            var currentUserId = _currentUserService.GetUserId();

            foreach (var id in ids)
            {
                try
                {
                    if (id == Guid.Empty)
                    {
                        responseList.Add(new RestoreResponseDTO
                        {
                            Id = id,
                            IsSuccess = false,
                            Message = "ID hồ sơ không hợp lệ!"
                        });
                        continue;
                    }

                    var result = await _repository.SoftDeleteAsync(id, currentUserId);
                    if (!result)
                    {
                        responseList.Add(new RestoreResponseDTO
                        {
                            Id = id,
                            IsSuccess = false,
                            Message = "Xóa hồ sơ sức khỏe thất bại!"
                        });
                        continue;
                    }

                    responseList.Add(new RestoreResponseDTO
                    {
                        Id = id,
                        IsSuccess = true,
                        Message = "Xóa hồ sơ sức khỏe thành công!"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa HealthProfile ID: {HealthProfileId}", id);
                    responseList.Add(new RestoreResponseDTO
                    {
                        Id = id,
                        IsSuccess = false,
                        Message = "Lỗi hệ thống: " + ex.Message
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResult<List<RestoreResponseDTO>>.Success(responseList, "Xử lý xóa danh sách hồ sơ sức khỏe hoàn tất");
        }
        public async Task<RestoreResponseDTO> RestoreHealthProfileAsync(Guid id, Guid? userId)
        {
            try
            {
                var restored = await _repository.RestoreAsync(id, userId);
                return new RestoreResponseDTO
                {
                    Id = id,
                    IsSuccess = restored,
                    Message = restored ? "Khôi phục hồ sơ sức khỏe thành công" : "Không thể khôi phục hồ sơ sức khỏe"
                };
            }
            catch (Exception ex)
            {
                return new RestoreResponseDTO { Id = id, IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<List<RestoreResponseDTO>> RestoreHealthProfileRangeAsync(List<Guid> ids, Guid? userId)
        {
            var results = new List<RestoreResponseDTO>();
            foreach (var id in ids)
            {
                results.Add(await RestoreHealthProfileAsync(id, userId));
            }
            return results;
        }
       
        #endregion
    }
}
