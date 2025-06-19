using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.NurseDTOs.Request;
using DTOs.NurseDTOs.Response;


namespace Repositories.Interfaces
{
    public interface INurseRepository
    {
        Task<bool> FindByEmailAsync(string email);
        Task AddAsync(User user);

        Task<NurseProfile> CreateNurseAsync(NurseProfile nurse);

        Task<NurseProfile?> GetNurseByUserIdAsync(Guid userId);

        Task<List<NurseProfile>> GetNurseAsync();
        Task<List<GetNurseDTO>> GetNurseDtoAsync();
        Task<bool> UpdateNurseAsync(UpdateNurseRequest request);
        Task<bool> SoftDeleteByNurseId(Guid nurseId);
    }
}
