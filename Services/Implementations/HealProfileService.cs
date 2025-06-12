using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;
using DTOs.StudentDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.Commons;

namespace Services.Implementations
{
    public class HealProfileService : BaseService<HealthProfile,Guid>, IHealProfileService
    {
        public HealProfileService(
            IGenericRepository<HealthProfile,
            Guid> repository,
            ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork
            ) : 
            base(repository, currentUserService, unitOfWork)
        {
        }

        public async Task<ApiResult<HealProfileResponseDTO>> CreateHealProfileAsync(CreateHealProfileRequestDTO request)
        {
            if (request == null)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Request cannot be null"));
            }

            var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
            var parent = await _unitOfWork.ParentRepository.GetParentByUserIdAsync(request.ParentId);

            if (student == null)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Student not found"));
            }

            if (student.ParentUserId != request.ParentId)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Parent ID does not match with the student's parent ID"));
            }

            if (parent == null)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Parent not found"));
            }

            // ✅ Query version max hiện tại
            int maxVersion = await _repository
                .GetQueryable()
                .Where(hp => hp.StudentId == request.StudentId)
                .MaxAsync(hp => (int?)hp.Version) ?? 0;

            // ✅ Convert DTO sang entity và gán Version
            HealthProfile healthProfile = HealProfileMappings.ToEntity(request);
            healthProfile.Version = maxVersion + 1;
            healthProfile.ProfileDate = DateTime.UtcNow;
            
            await _repository.AddAsync(healthProfile);
            await _unitOfWork.SaveChangesAsync();

            GetAllStudentDTO studentdto = StudentMappings.ToGetAllStudent(student);
            var response = HealProfileMappings.FromEntity(healthProfile);
            response.StudentInformation = studentdto;

            return ApiResult<HealProfileResponseDTO>.Success(response, "Tạo hồ sơ sức khỏe thành công!!!");
        }


        public async Task<ApiResult<bool>> DeleteHealProfileAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfileByStudentIdAsync(Guid studentId)
        {

            if (await _unitOfWork.StudentRepository.GetByIdAsync(studentId) == null)
            {
                return ApiResult<List<HealProfileResponseDTO>>.Failure(
                    new Exception("Không tìm thấy học sinh với ID này."));
            }

            var profiles = await _unitOfWork.HealProfileRepository
                .GetAllAsync(p => p.StudentId == studentId);

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


        public async Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfilesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByIdAsync(Guid id)
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

        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByParentIdAsync(Guid parentId)
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

        public async Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByStudentIdAsync(Guid studentId)
        {
            var profiles = await _unitOfWork.HealProfileRepository
                .GetAllAsync(p => p.StudentId == studentId);
            if (profiles == null || !profiles.Any())
            {
                return ApiResult<HealProfileResponseDTO>.Failure(
                    new Exception("Không tìm thấy hồ sơ sức khỏe nào dựa trên id học sinh này."));
            }
            // Giả sử chỉ lấy hồ sơ đầu tiên nếu có nhiều
            var profile = profiles.First();
            var response = HealProfileMappings.FromEntity(profile);
            return ApiResult<HealProfileResponseDTO>.Success(response, "Lấy thông tin hồ sơ sức khỏe thành công.");

        }

        public async Task<ApiResult<bool>> SoftDeleteHealProfileAsync(Guid id)
        {
            var currentUserId = _currentUserService.GetUserId();
            var result = await _repository.SoftDeleteAsync(id, currentUserId);
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<bool>.Success(result, "Xóa hồ sơ sức khỏe thành công");
        }

        public async Task<ApiResult<HealProfileResponseDTO>> UpdateHealProfileByIdAsync(Guid id, UpdateHealProfileRequestDTO request)
        {
            var entity = await _unitOfWork.HealProfileRepository.GetByIdAsync(id);

            if (entity == null)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception("Không tìm thấy hồ sơ sức khỏe."));
            }
            if (entity.Version != request.Version)
            {
                return ApiResult<HealProfileResponseDTO>.Failure(new Exception($"Dữ liệu đã được cập nhật trước đó. Phiên bản hiện tại là {entity.Version}."));
            }

            // Cập nhật các field từ DTO
            entity.Version = request.Version;
            entity.ProfileDate = request.ProfileDate;

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
            //SetAuditFieldsForUpdate(entity);


            await _unitOfWork.SaveChangesAsync();

            // Map sang ResponseDTO với string enums
            var response = HealProfileMappings.FromEntity(entity);
            return ApiResult<HealProfileResponseDTO>.Success(response,"Cập nhật thông tin hồ sơ sức khỏe thành công");
        }
    }
}
