using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class ParentMedicationDeliveryRepository : GenericRepository<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryRepository
    {

        public ParentMedicationDeliveryRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }
        
    }
}
