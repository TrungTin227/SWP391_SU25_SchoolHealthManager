using BusinessObjects;
using DTOs.MedicationLotDTOs.Response;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class StudentService :BaseService<Student,Guid>, IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public StudentService(
            IGenericRepository<Student, Guid> repository, 
            ICurrentUserService currentUserService, IUnitOfWork unitOfWork, 
            ICurrentTime currentTime) : 
            base(repository, 
                currentUserService, 
                unitOfWork, 
                currentTime)
        {
        }

        public async Task<ApiResult<AddStudentRequestDTO>> AddStudentAsync(AddStudentRequestDTO addStudentRequestDTO)
        {
            try
            {
                if (addStudentRequestDTO == null)
                    return ApiResult<AddStudentRequestDTO>.Failure(new ArgumentNullException(nameof(addStudentRequestDTO)));

                var studentcode = await _unitOfWork.StudentRepository.CheckIfStudentCodeExistsAsync(addStudentRequestDTO.StudentCode);
                if (studentcode == true)
                {
                    return ApiResult<AddStudentRequestDTO>.Failure(new Exception("Mã học sinh đã tồn tại!!"));
                }

                if (addStudentRequestDTO.ParentID == Guid.Empty)
                {
                    return ApiResult<AddStudentRequestDTO>.Failure(new Exception("Yêu cầu nhập ID phụ huynh!!"));
                }

                if (await _unitOfWork.ParentRepository.GetParentByUserIdAsync(addStudentRequestDTO.ParentID) == null)
                {
                    return ApiResult<AddStudentRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID này: " + addStudentRequestDTO.ParentID + " !!"));
                }

                await _unitOfWork.StudentRepository.AddAsync(addStudentRequestDTO.AddStudentToStudent());
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<AddStudentRequestDTO>.Success(addStudentRequestDTO, "Thêm học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo học sinh");
                return ApiResult<AddStudentRequestDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<Student>>> GetAllStudentsAsync()
        {
            try
            {
                var students = await _unitOfWork.StudentRepository.GetAllStudentsAsync();
                return ApiResult<List<Student>>.Success(students, "Lấy danh sách học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách học sinh");
                return ApiResult<List<Student>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<GetAllStudentDTO>>> GetAllStudentsDTOAsync()
        {
            try
            {
                var students = await _unitOfWork.StudentRepository.GetAllStudentsDTOAsync();
                if (students == null || !students.Any())
                {
                    return ApiResult<List<GetAllStudentDTO>>.Failure(new Exception("Không có học sinh nào được tìm thấy."));
                }
                return ApiResult<List<GetAllStudentDTO>>.Success(students, "Lấy danh sách học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách học sinh");
                return ApiResult<List<GetAllStudentDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<GetAllStudentDTO?>> GetStudentByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return ApiResult<GetAllStudentDTO?>.Failure(new ArgumentNullException(nameof(id), "ID học sinh không được để trống."));
                }
                var student = await _unitOfWork.StudentRepository.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return ApiResult<GetAllStudentDTO?>.Failure(new Exception("Không tìm thấy học sinh với ID: " + id + " !!"));
                }
                return ApiResult<GetAllStudentDTO?>.Success(student, "Lấy thông tin học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin học sinh theo ID");
                return ApiResult<GetAllStudentDTO?>.Failure(ex);
            }
        }

        public async Task<ApiResult<GetAllStudentDTO?>> GetStudentByStudentCodeAsync(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                {
                    return ApiResult<GetAllStudentDTO?>.Failure(new ArgumentNullException(nameof(studentCode), "Mã học sinh không được để trống."));
                }
                var student = await _unitOfWork.StudentRepository.GetStudentByStudentCode(studentCode);
                if (student == null)
                {
                    return ApiResult<GetAllStudentDTO?>.Failure(new Exception("Không tìm thấy học sinh với mã: " + studentCode + " !!"));
                }
                return ApiResult<GetAllStudentDTO?>.Success(student, "Lấy thông tin học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin học sinh theo mã");
                return ApiResult<GetAllStudentDTO?>.Failure(ex);
            }
        }

        public async Task<ApiResult<UpdateStudentRequestDTO>> UpdateStudentById(UpdateStudentRequestDTO updateStudentRequestDTO)
        {
            try
            {
               
                if (updateStudentRequestDTO.Id == Guid.Empty)
                {
                    return ApiResult<UpdateStudentRequestDTO>.Failure(new ArgumentNullException(nameof(updateStudentRequestDTO.Id), "ID học sinh không được để trống."));
                }
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(updateStudentRequestDTO.Id);
                if (student == null)
                {
                    return ApiResult<UpdateStudentRequestDTO>.Failure(new Exception("Không tìm thấy học sinh với ID: " + updateStudentRequestDTO.Id + " !!"));
                }
                student.UpdatedBy = _currentUserService.GetUserId() ?? SystemGuid;
                student.UpdatedAt = DateTime.UtcNow;
                var updated = await _unitOfWork.StudentRepository.UpdateStudentAsync(updateStudentRequestDTO.ToUpdatedStudent(student));
                if (!updated)
                {
                    return ApiResult<UpdateStudentRequestDTO>.Failure(new Exception("Cập nhật học sinh thất bại!!"));
                }
                return ApiResult<UpdateStudentRequestDTO>.Success(updateStudentRequestDTO, "Cập nhật học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin học sinh");
                return ApiResult<UpdateStudentRequestDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteStudentByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return ApiResult<bool>.Failure(new ArgumentException("ID học sinh không được để trống."));

                var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
                if (student == null)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy học sinh với ID: " + id));
                return await SoftDeleteStudentByIdsAsync(new List<Guid> { id });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteStudentByCodeAsync(string studentCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(studentCode))
                    return ApiResult<bool>.Failure(new ArgumentException("Mã học sinh không được để trống."));

                return await SoftDeleteStudentByCodesAsync(new List<string> { studentCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh theo mã");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<GetAllStudentDTO>>> GetStudentsByParentIdAsync(Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return ApiResult<List<GetAllStudentDTO>>.Failure(new ArgumentNullException(nameof(parentId), "ID phụ huynh không được để trống."));
                }
                var students = await _unitOfWork.StudentRepository.GetStudentsByParentIdAsync(parentId);
                if (students == null || !students.Any())
                {
                    return ApiResult<List<GetAllStudentDTO>>.Failure(new Exception("Không tìm thấy học sinh nào cho phụ huynh với ID: " + parentId + " !!"));
                }
                return ApiResult<List<GetAllStudentDTO>>.Success(students, "Lấy danh sách học sinh theo phụ huynh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách học sinh theo ID phụ huynh");
                return ApiResult<List<GetAllStudentDTO>>.Failure(ex);
            }

        }

        public async Task<ApiResult<bool>> SoftDeleteStudentByIdsAsync(List<Guid> studentIds)
        {
            try
            {
                if (studentIds == null || !studentIds.Any())
                {
                    return ApiResult<bool>.Failure(new ArgumentException("ID học sinh không được để trống."));
                }

                var systemUserId = _currentUserService.GetUserId() ?? SystemGuid;
                var vietnamTime = _currentTime.GetVietnamTime();

                var students = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .Where(s => studentIds.Contains(s.Id) && !s.IsDeleted)
                    .ToListAsync();

                var notFoundIds = studentIds.Except(students.Select(s => s.Id)).ToList();
                if (notFoundIds.Any())
                {
                    return ApiResult<bool>.Failure(new Exception($"Không tìm thấy các học sinh với ID: {string.Join(", ", notFoundIds)}"));
                }

                foreach (var student in students)
                {
                    student.IsDeleted = true;
                    student.DeletedBy = systemUserId;
                    student.DeletedAt = vietnamTime;
                }

                await _unitOfWork.StudentRepository.UpdateRangeAsync(students);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, $"Đã xóa mềm {students.Count} học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh");
                return ApiResult<bool>.Failure(new Exception("Xóa danh sách học sinh thất bại: " + ex.Message));
            }
        }

        public async Task<ApiResult<bool>> SoftDeleteStudentByCodesAsync(List<string> studentCodes)
        {
            try
            {
                if (studentCodes == null || !studentCodes.Any())
                {
                    return ApiResult<bool>.Failure(new ArgumentException("Mã học sinh không được để trống."));
                }

                var normalizedCodes = studentCodes.Select(c => c.Trim()).ToList();
                var systemUserId = _currentUserService.GetUserId() ?? SystemGuid;
                var vietnamTime = _currentTime.GetVietnamTime();

                var students = await _unitOfWork.StudentRepository
                    .GetQueryable()
                    .Where(s => normalizedCodes.Contains(s.StudentCode) && !s.IsDeleted)
                    .ToListAsync();

                var notFoundCodes = normalizedCodes.Except(students.Select(s => s.StudentCode)).ToList();
                if (notFoundCodes.Any())
                {
                    return ApiResult<bool>.Failure(new Exception($"Không tìm thấy học sinh với mã: {string.Join(", ", notFoundCodes)}"));
                }

                foreach (var student in students)
                {
                    student.IsDeleted = true;
                    student.DeletedBy = systemUserId;
                    student.DeletedAt = vietnamTime;
                }

                await _unitOfWork.StudentRepository.UpdateRangeAsync(students);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, $"Đã xóa mềm {students.Count} học sinh thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh");
                return ApiResult<bool>.Failure(new Exception("Xóa danh sách học sinh thất bại: " + ex.Message));
            }
        }

    }
}
