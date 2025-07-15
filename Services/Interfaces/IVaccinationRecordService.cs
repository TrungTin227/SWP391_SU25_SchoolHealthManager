using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;

namespace Services.Interfaces
{
    public interface IVaccinationRecordService
    {
        Task<ApiResult<CreateVaccinationRecordResponse>> CreateAsync(CreateVaccinationRecordRequest request);
        Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request);
        Task<ApiResult<bool>> DeleteAsync(Guid id, Guid deletedBy);
        Task<ApiResult<CreateVaccinationRecordResponse>> GetByIdAsync(Guid id);

        Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetRecordsByScheduleAsync(
             Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null);

        Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetRecordsByStudentAsync(
            Guid studentId, int pageNumber, int pageSize, string? searchTerm = null);
        Task<ApiResult<PagedList<CreateVaccinationRecordResponse>>> GetAllAsync(int pageNumber, int pageSize);
    }
}
