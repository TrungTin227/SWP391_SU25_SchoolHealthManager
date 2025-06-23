using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class VaccinationRecordService : IVaccinationRecordService
    {
        private readonly IVaccinationRecordRepository _repository;

        public VaccinationRecordService(IVaccinationRecordRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<VaccinationRecordResponse>> GetAllAsync()
        {
            return _repository.GetAllAsync();
        }

        public Task<VaccinationRecordResponse?> GetByIdAsync(Guid id)
        {
            return _repository.GetByIdAsync(id);
        }

        public Task<VaccinationRecordResponse> CreateAsync(CreateVaccinationRecordRequest request)
        {
            return _repository.CreateAsync(request);
        }

        public Task<VaccinationRecordResponse?> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request)
        {
            return _repository.UpdateAsync(id, request);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return _repository.DeleteAsync(id);
        }
    }
}
