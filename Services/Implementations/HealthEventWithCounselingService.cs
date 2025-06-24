using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class HealthEventWithCounselingService : IHealthEventWithCounselingService
    {
        private readonly IHealthEventWithCounselingRepository _repository;

        public HealthEventWithCounselingService(IHealthEventWithCounselingRepository repository)
        {
            _repository = repository;
        }

        public async Task<HealthEventWithCounselingResponse?> CreateWithCounselingAsync(HealthEventCreateWithCounselingRequest request)
        {
            return await _repository.CreateWithCounselingAsync(request);
        }

        public async Task<HealthEventWithCounselingResponse?> GetByIdWithCounselingAsync(Guid id)
        {
            return await _repository.GetByIdWithCounselingAsync(id);
        }

        public async Task<List<HealthEventWithCounselingResponse>> GetByStudentIdAsync(Guid studentId)
        {
            return await _repository.GetByStudentIdAsync(studentId);
        }

        public async Task<List<HealthEventWithCounselingResponse>> GetByStudentIdWithCounselingAsync(Guid studentId)
        {
            return await _repository.GetByStudentIdWithCounselingAsync(studentId);
        }
    }
}
