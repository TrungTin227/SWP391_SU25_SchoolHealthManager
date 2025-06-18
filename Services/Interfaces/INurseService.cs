using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.NurseDTOs.Request;
using DTOs.NurseDTOs.Response;

namespace Services.Interfaces
{
    public interface INurseService
    {
        Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user);
        Task<ApiResult<AddNurseRequestDTO>> CreateNurseAsync(AddNurseRequestDTO request);
        Task<ApiResult<List<GetNurseDTO>>> GetAllNursesAsync();
        Task<ApiResult<bool>> UpdateNurseAsync(UpdateNurseRequest request);
        Task<ApiResult<bool>> SoftDeleteByNurseIdAsync(Guid NurseId);
        Task<ApiResult<UserRegisterRespondDTO>> RegisterNurseUserAsync(UserRegisterRequestDTO user);
    }
}
