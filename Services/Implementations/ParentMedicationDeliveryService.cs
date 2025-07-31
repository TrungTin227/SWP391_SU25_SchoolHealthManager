using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using Repositories;
using Repositories.Interfaces;
using Services.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class ParentMedicationDeliveryService : BaseService<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryService
    {
        private readonly SchoolHealthManagerDbContext _dbContext;
        private readonly ISchoolHealthEmailService _emailService;

        // Constructor sử dụng base service để khởi tạo repository, current user service và unit of work
        public ParentMedicationDeliveryService(
            IGenericRepository<ParentMedicationDelivery, Guid> parentMedicationDeliveryRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            SchoolHealthManagerDbContext dbContext,
            ICurrentTime currentTime,
            ISchoolHealthEmailService emailService
            )
        :
            base(parentMedicationDeliveryRepository, currentUserService, unitOfWork, currentTime)
        {
            _dbContext = dbContext;
            _emailService = emailService;
        }

        
    }
}
