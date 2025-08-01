using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryDetailService : BaseService<ParentMedicationDeliveryDetail, Guid>, IParentMedicationDeliveryDetailService
    {
        public ParentMedicationDeliveryDetailService(IGenericRepository<ParentMedicationDeliveryDetail, Guid> repository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ICurrentTime currentTime) : base(repository, currentUserService, unitOfWork, currentTime)
        {
        }
    }
}
