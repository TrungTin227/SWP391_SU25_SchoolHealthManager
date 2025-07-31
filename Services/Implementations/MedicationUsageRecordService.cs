using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class MedicationUsageRecordService : BaseService<MedicationUsageRecord, Guid>, IMedicationUsageRecordService
    {
        public MedicationUsageRecordService(IGenericRepository<MedicationUsageRecord, Guid> repository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ICurrentTime currentTime) : base(repository, currentUserService, unitOfWork, currentTime)
        {
        }
    }
}
