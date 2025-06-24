using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;

namespace Services.Interfaces
{
    public interface IHealthEventWithCounselingService
    {
        Task<HealthEventWithCounselingResponse?> CreateWithCounselingAsync(HealthEventCreateWithCounselingRequest request);
        Task<HealthEventWithCounselingResponse?> GetByIdWithCounselingAsync(Guid id);
        Task<List<HealthEventWithCounselingResponse>> GetByStudentIdAsync(Guid studentId);
        Task<List<HealthEventWithCounselingResponse>> GetByStudentIdWithCounselingAsync(Guid studentId);
    }
}
