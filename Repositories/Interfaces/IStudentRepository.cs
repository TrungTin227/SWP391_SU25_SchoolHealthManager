using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IStudentRepository : IGenericRepository<Student, Guid>
    {
        Task<bool> CheckIfStudentCodeExistsAsync(string code);
        Task<List<Student>> GetAllStudentsAsync();
        Task<List<GetAllStudentDTO>> GetAllStudentsDTOAsync();

        Task<GetAllStudentDTO?> GetStudentByStudentCode(string studentCode);

        Task<GetAllStudentDTO?> GetStudentByIdAsync(Guid id);
        Task<bool> UpdateStudentAsync(Student student);
        Task<bool> SoftDeleteStudentAsync(Student student);

        Task<List<GetAllStudentDTO>> GetStudentsByParentIdAsync(Guid id);


    }
}
