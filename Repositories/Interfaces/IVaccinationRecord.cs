using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IVaccinationRecord
    {
        Task<IEnumerable<VaccinationRecordResponse>> GetAllAsync();
        Task<VaccinationRecordResponse?> GetByIdAsync(Guid id);
        Task<VaccinationRecordResponse> CreateAsync(CreateVaccinationRecordRequest request);
        Task<VaccinationRecordResponse?> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
