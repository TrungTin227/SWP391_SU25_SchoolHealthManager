using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;

namespace Services.Interfaces
{
    public interface IParentService
    {
        Task<ApiResult<UserRegisterRespondDTO>> RegisterUserAsync(UserRegisterRequestDTO user);
        Task<ApiResult<AddParentRequestDTO>> CreateParentAsync(AddParentRequestDTO request);
        Task<ApiResult<List<GetAllParentDTO>>> GetAllParentsAsync();

    }
}
