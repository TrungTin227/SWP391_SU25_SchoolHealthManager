using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IParentRepository
    {
        Task<User> FindByEmailAsync(string email);
        Task AddAsync(User user);

    }
}
