using DTOs.GlobalDTO.Respond;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IHealProfileService 
    {
         Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByIdAsync(Guid id);
         Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByStudentCodeAsync(string studentcode);
         Task<ApiResult<HealProfileResponseDTO>> GetNewestHealProfileByStudentCodeAsync(string studentcode);

         Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByParentIdAsync(Guid parentId);
         Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfileByStudentCodeAsync(string studentcode);
         Task<ApiResult<HealProfileResponseDTO>> CreateHealProfileAsync(CreateHealProfileRequestDTO request);
         Task<ApiResult<HealProfileResponseDTO>> UpdateHealProfileByStudentCodeAsync(string studentcode, UpdateHealProfileRequestDTO request);
         Task<ApiResult<RestoreResponseDTO>> SoftDeleteHealthProfileAsync(Guid id);
        Task<ApiResult<List<RestoreResponseDTO>>> SoftDeleteHealthProfilesAsync(List<Guid> ids);
        Task<RestoreResponseDTO> RestoreHealthProfileAsync(Guid id, Guid? userId);
        Task<List<RestoreResponseDTO>> RestoreHealthProfileRangeAsync(List<Guid> ids, Guid? userId);

    }
}
