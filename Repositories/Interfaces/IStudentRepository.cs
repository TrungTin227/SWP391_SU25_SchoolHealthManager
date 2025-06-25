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

        Task<List<Student>> GetStudentsByGradeAndSectionAsync(List<string> grades, List<string> sections);
        Task<List<GetAllStudentDTO>> GetStudentsDTOByGradeAndSectionAsync(List<string> grades, List<string> sections);

        // Method hỗ trợ thêm cho scheduling
        Task<List<Student>> GetStudentsByIdsAsync(List<Guid> studentIds);
        Task<List<GetAllStudentDTO>> GetStudentsDTOByIdsAsync(List<Guid> studentIds);

        // Method để lấy danh sách Grade và Section có sẵn
        Task<List<string>> GetAvailableGradesAsync();
        Task<List<string>> GetAvailableSectionsAsync();
        Task<Dictionary<string, List<string>>> GetGradeSectionMappingAsync();
    }
}
