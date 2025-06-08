using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;

namespace Services.Interfaces
{
    public interface IStudentService
    {
        Task<ApiResult<AddStudentRequestDTO>> AddStudentAsync(AddStudentRequestDTO addStudentRequestDTO);
    }
}
