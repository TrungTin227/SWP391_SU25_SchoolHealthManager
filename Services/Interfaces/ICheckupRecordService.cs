using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using DTOs.GlobalDTO.Respond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICheckupRecordService
    {
        public Task<ApiResult<CheckupRecordRespondDTO>> CreateCheckupRecordAsync(CreateCheckupRecordRequestDTO request);
        //public Task<ApiResult<CheckupRecordRespondDTO>> UpdateCheckupRecordAsync(UpdateCheckupRecordRequestDTO request);
        public Task<ApiResult<CheckupRecordRespondDTO?>> GetByIdAsync(Guid id);
        public Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStaffIdAsync(Guid id);
        public Task<ApiResult<List<CheckupRecordRespondDTO?>>> GetAllByStudentCodeAsync(string studentCode);
        public Task<ApiResult<bool>> SoftDeleteAsync(Guid id);
        public Task<ApiResult<bool>> SoftDeleteRangeAsync(List<Guid> ids);
        Task<RestoreResponseDTO> RestoreCheckupRecordAsync(Guid id, Guid? userId);
        Task<List<RestoreResponseDTO>> RestoreCheckupRecordRangeAsync(List<Guid> ids, Guid? userId);

    }
}
