using DTOs.NurseProfile.Request;
using DTOs.NurseProfile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface INurseProfileService
    {
        Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user);
        Task<ApiResult<AddNurseRequest>> CreateNurseAsync(AddNurseRequest request);
        Task<ApiResult<List<NurseProfileRespondDTOs>>> GetAllNursesAsync();
        Task<ApiResult<bool>> UpdateNurseAsync(UpdateNurseRequest request);
        Task<ApiResult<bool>> SoftDeleteByNurseIdAsync(Guid NurseId);
        Task<ApiResult<UserRegisterRespondDTO>> RegisterNurseUserAsync(UserRegisterRequestDTO user);
    }
}
