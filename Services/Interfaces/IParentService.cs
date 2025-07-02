using DTOs.GlobalDTO.Respond;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IParentService
    {
        Task<ApiResult<List<GetAllParentDTO>>> GetAllParentsAsync();
        Task<ApiResult<bool>> UpdateRelationshipByParentIdAsync(UpdateRelationshipByParentId request);
        Task<ApiResult<bool>> SoftDeleteByParentIdAsync(Guid parentId);
        Task<ApiResult<UserRegisterRespondDTO>> RegisterParentUserAsync(UserRegisterRequestDTO user);
        Task<RestoreResponseDTO> RestoreParentAsync(Guid id, Guid? userId);
        Task<List<RestoreResponseDTO>> RestoreParentRangeAsync(List<Guid> ids, Guid? userId);

        Task<ApiResult<List<RestoreResponseDTO>>> SoftDeleteByParentIdListAsync(List<Guid> parentIds);

    }
}
