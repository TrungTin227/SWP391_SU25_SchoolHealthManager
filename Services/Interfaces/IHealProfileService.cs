using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;

namespace Services.Interfaces
{
    public interface IHealProfileService 
    {
        public Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfilesAsync();
        public Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByIdAsync(Guid id);
        public Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByStudentIdAsync(Guid studentId);
        public Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByParentIdAsync(Guid parentId);
        public Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfileByStudentIdAsync(Guid studentId);

        public Task<ApiResult<HealProfileResponseDTO>> CreateHealProfileAsync(CreateHealProfileRequestDTO request);
        public Task<ApiResult<HealProfileResponseDTO>> UpdateHealProfileByIdAsync(Guid id, UpdateHealProfileRequestDTO request);
        public Task<ApiResult<bool>> DeleteHealProfileAsync(Guid id);
        public Task<ApiResult<bool>> SoftDeleteHealProfileAsync(Guid id);

    }
}
