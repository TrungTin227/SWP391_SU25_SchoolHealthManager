using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class CheckupRecordService : BaseService<CheckupRecordService, Guid>, ICheckupRecordService
    {
        public CheckupRecordService(
            IGenericRepository<CheckupRecordService, Guid> repository
            , ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork, 
            ICurrentTime currentTime) : 
            base(repository, 
                currentUserService, 
                unitOfWork, 
                currentTime)
        {
        }


    }
}
