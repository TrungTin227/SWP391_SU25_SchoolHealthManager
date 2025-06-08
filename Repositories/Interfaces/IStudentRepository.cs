using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;

namespace Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student, Guid>
    {
        Task<bool> CheckIfStudentCodeExistsAsync(string code);
    }
}
