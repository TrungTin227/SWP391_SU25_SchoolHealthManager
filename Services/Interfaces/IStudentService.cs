using DTOs.GlobalDTO.Respond;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResult<AddStudentRequestDTO>> AddStudentAsync(AddStudentRequestDTO addStudentRequestDTO);

        Task<ApiResult<List<Student>>> GetAllStudentsAsync();
        Task<ApiResult<List<GetAllStudentDTO>>> GetAllStudentsDTOAsync();
        Task<ApiResult<GetAllStudentDTO?>> GetStudentByStudentCodeAsync(string studentCode);

        Task<ApiResult<GetAllStudentDTO?>> GetStudentByIdAsync(Guid id);
        Task<ApiResult<UpdateStudentRequestDTO>> UpdateStudentById(UpdateStudentRequestDTO updateStudentRequestDTO);
        Task<ApiResult<bool>> SoftDeleteStudentByIdAsync(Guid id);
        Task<ApiResult<bool>> SoftDeleteStudentByIdsAsync(List<Guid> id);
        Task<ApiResult<bool>> SoftDeleteStudentByCodeAsync(string studentCode);
        Task<ApiResult<bool>> SoftDeleteStudentByCodesAsync(List<string> studentCode);
        Task<ApiResult<List<GetAllStudentDTO>>> GetStudentsByParentIdAsync(Guid parentId);
        Task<RestoreResponseDTO> RestoreStudentAsync(Guid id, Guid? userId);
        Task<List<RestoreResponseDTO>> RestoreStudentRangeAsync(List<Guid> ids, Guid? userId);
    }
}
