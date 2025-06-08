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
        public StudentService(IUnitOfWork unitOfWork, ILogger<StudentService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
                    return ApiResult<AddStudentRequestDTO>.Failure(new Exception("Không tìm thấy phụ huynh với ID này: "+addStudentRequestDTO.ParentID+" !!"));
                }

                await _unitOfWork.StudentRepository.AddAsync(addStudentRequestDTO.ToStudent());
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
                var students =await _unitOfWork.StudentRepository.GetAllStudentsAsync();
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
                return ApiResult<List<GetAllStudentDTO>>.Success(students, "Lấy danh sách học sinh thành công!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách học sinh");
                return ApiResult<List<GetAllStudentDTO>>.Failure(ex);
            }
        }
    }
}
