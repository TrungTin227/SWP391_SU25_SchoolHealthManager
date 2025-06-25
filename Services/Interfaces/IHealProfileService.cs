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
        public Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByStudentCodeAsync(string studentcode);
        public Task<ApiResult<HealProfileResponseDTO>> GetNewestHealProfileByStudentCodeAsync(string studentcode);

        public Task<ApiResult<HealProfileResponseDTO>> GetHealProfileByParentIdAsync(Guid parentId);
        public Task<ApiResult<List<HealProfileResponseDTO>>> GetAllHealProfileByStudentCodeAsync(string studentcode);
        public Task<ApiResult<HealProfileResponseDTO>> CreateHealProfileAsync(CreateHealProfileRequestDTO request);
        public Task<ApiResult<HealProfileResponseDTO>> UpdateHealProfileByStudentCodeAsync(string studentcode, UpdateHealProfileRequestDTO request);
        public Task<ApiResult<bool>> DeleteHealProfileAsync(Guid id);
        public Task<ApiResult<bool>> SoftDeleteHealProfileAsync(Guid id);

    }
}
