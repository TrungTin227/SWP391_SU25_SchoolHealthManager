using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentDTOs.Request;
using DTOs.ParentDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IParentRepository
    {
        Task<bool> FindByEmailAsync(string email);
        Task AddAsync(User user);

        Task<Parent> CreateParentAsync(Parent parent);

        Task<Parent?> GetParentByUserIdAsync(Guid userId);

        Task<List<Parent>> GetAllParentAsync();
        Task<List<GetAllParentDTO>> GetAllParentDtoAsync();
        Task<bool>  UpdateRelationshipByParentIdAsync(UpdateRelationshipByParentId request);
        Task<bool> SoftDeleteByParentId(Guid parentId);

    }
}
