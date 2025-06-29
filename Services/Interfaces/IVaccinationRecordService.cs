using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;

namespace Services.Interfaces
{
    public interface IVaccinationRecordService
    {
        Task<ApiResult<CreateVaccinationRecordResponse>> CreateAsync(CreateVaccinationRecordRequest request);
        Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request);
        Task<ApiResult<bool>> DeleteAsync(Guid id, Guid deletedBy);
        Task<ApiResult<VaccinationRecord?>> GetByIdAsync(Guid id);

        // Get by Schedule
        Task<ApiResult<PagedList<VaccinationRecord>>> GetRecordsByScheduleAsync(Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null);

        // Get by Student
        Task<ApiResult<PagedList<VaccinationRecord>>> GetRecordsByStudentAsync(Guid studentId, int pageNumber, int pageSize, string? searchTerm = null);
    }
}
