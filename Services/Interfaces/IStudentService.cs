using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;

namespace Services.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResult<AddStudentRequestDTO>> AddStudentAsync(AddStudentRequestDTO addStudentRequestDTO);

        Task<ApiResult<List<Student>>> GetAllStudentsAsync();
        Task<ApiResult<List<GetAllStudentDTO>>> GetAllStudentsDTOAsync();

    }
}
