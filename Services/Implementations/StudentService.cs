using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.MedicationLotDTOs.Response;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public StudentService(IUnitOfWork unitOfWork, ILogger<StudentService> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
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
                {
                    return ApiResult<bool>.Failure(new ArgumentNullException(nameof(id), "ID học sinh không được để trống."));
                }
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
                if (student == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy học sinh với ID: " + id + " !!"));
                }
                student.IsDeleted = true;
                student.DeletedBy = _currentUserService.GetUserId() ?? SystemGuid;
                student.DeletedAt = DateTime.UtcNow;
                
                var result = await _unitOfWork.StudentRepository.SoftDeleteStudentAsync(student);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Xóa học sinh thất bại!!"));
                }
                
                return ApiResult<bool>.Success(true, "Xóa học sinh thành công!!");
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
                {
                    return ApiResult<bool>.Failure(new ArgumentNullException(nameof(studentCode), "Mã học sinh không được để trống."));
                }
                var student = await _unitOfWork.StudentRepository.FirstOrDefaultAsync(o=> o.StudentCode == studentCode);
                if (student == null)
                {
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy học sinh với mã: " + studentCode + " !!"));
                }
                student.DeletedBy = _currentUserService.GetUserId() ?? SystemGuid;
                student.DeletedAt = DateTime.UtcNow;
                var result = await _unitOfWork.StudentRepository.SoftDeleteStudentAsync(student);
                if (!result)
                {
                    return ApiResult<bool>.Failure(new Exception("Xóa học sinh thất bại!!"));
                }
                return ApiResult<bool>.Success(true, "Xóa học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa học sinh theo mã");
                return ApiResult<bool>.Failure(ex);
            }
        }
    }
}
