using DTOs.NurseProfile.Request;
using DTOs.NurseProfile.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface INurseProfileRepository : IGenericRepository<NurseProfile, Guid>
    {
        Task<bool> FindByEmailAsync(string email);
        Task AddAsync(User user);

        Task<NurseProfile> CreateNurseAsync(NurseProfile nurse);

        Task<NurseProfile?> GetNurseByUserIdAsync(Guid userId);

        Task<List<NurseProfile>> GetNurseAsync();
        Task<List<NurseProfileRespondDTOs>> GetNurseDtoAsync();
        Task<bool> UpdateNurseAsync(UpdateNurseRequest request);
        Task<bool> SoftDeleteByNurseId(Guid nurseId);
    }
}
