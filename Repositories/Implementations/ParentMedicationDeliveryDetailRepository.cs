using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class ParentMedicationDeliveryDetailRepository : GenericRepository<ParentMedicationDeliveryDetail, Guid>, IParentMedicationDeliveryDetailRepository
    {
        public ParentMedicationDeliveryDetailRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
    }
}
