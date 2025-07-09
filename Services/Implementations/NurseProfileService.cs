using DTOs.NurseProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class NurseProfileService : BaseService<NurseProfile, Guid>, INurseProfileService
    {
        public NurseProfileService(IGenericRepository<NurseProfile, Guid> repository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ICurrentTime currentTime) : base(repository, currentUserService, unitOfWork, currentTime)
        {
        }

        public async Task<ApiResult<List<NurseProfileRespondDTOs>>> GetAllNurseAsync()
        {
            try
            {
                var result = await _repository.GetAllAsync(
                    predicate: x => !x.IsDeleted,
                    includes: x => x.User
                );

                var respondDtos = NurseProfileMappings.ToRespondDTOList(result.ToList());
                if (respondDtos == null || !respondDtos.Any())
                {
                    return ApiResult<List<NurseProfileRespondDTOs>>.Failure(new Exception("Không có nurse nào hiện có!!"));
                }

                return ApiResult<List<NurseProfileRespondDTOs>>.Success(respondDtos, "Lấy tất cả y tá thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<NurseProfileRespondDTOs>>.Failure(ex);
            }
        }

    }
}
