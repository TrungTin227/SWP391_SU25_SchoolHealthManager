using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class StudentRepository : GenericRepository<Student, Guid>, IStudentRepository
    {
        private readonly SchoolHealthManagerDbContext _context;
        public StudentRepository(SchoolHealthManagerDbContext context) : base(context)
        {
            _context = context;
        }

        public new async Task<Student> AddAsync(Student entity)
        {
            var now = DateTime.UtcNow;
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
            entity.IsDeleted = false;
            entity.ParentUserId = entity.ParentUserId;
            await _context.Students.AddAsync(entity);
            return entity;
        }

        public async Task<bool> CheckIfStudentCodeExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return await _context.Students
                .AsNoTracking()
                .AnyAsync(u => u.StudentCode == code);
        }

        public Task<List<Student>> GetAllStudentsAsync()
        {
            return _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Grade)
                .ToListAsync();
        }

        public async Task<List<GetAllStudentDTO>> GetAllStudentsDTOAsync()
        {
            return await _context.Students
                   .Where(s => !s.IsDeleted)
                   .OrderBy(s => s.Grade)
                   .Select(s => new GetAllStudentDTO
                   {
                       Id = s.Id,
                       StudentCode = s.StudentCode,
                       FirstName = s.FirstName,
                       LastName = s.LastName,
                       DateOfBirth = s.DateOfBirth,
                       Grade = s.Grade,
                       Section = s.Section,
                       Image = s.Image
                   })
                   .ToListAsync();
        }

        public async Task<GetAllStudentDTO?> GetStudentByIdAsync(Guid id)
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new GetAllStudentDTO
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Grade = s.Grade,
                    Section = s.Section,
                    Image = s.Image
                })
                .FirstOrDefaultAsync();
        }

        public async Task<GetAllStudentDTO?> GetStudentByStudentCode(string studentCode)
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => s.StudentCode == studentCode && !s.IsDeleted)
                .Select(s => new GetAllStudentDTO
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Grade = s.Grade,
                    Section = s.Section,
                    Image = s.Image
                })
                .FirstOrDefaultAsync();
        }

        public Task<List<GetAllStudentDTO>> GetStudentsByParentIdAsync(Guid id)
        {
            return _context.Students
                .AsNoTracking()
                .Where(s => s.ParentUserId == id && !s.IsDeleted)
                .OrderBy(s => s.Grade)
                .Select(s => new GetAllStudentDTO
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Grade = s.Grade,
                    Section = s.Section,
                    Image = s.Image
                })
                .ToListAsync();
        }

        public async Task<bool> SoftDeleteStudentAsync(Student student)
        {
            if (student == null)
                return false;
            student.IsDeleted = true;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStudentAsync(Student updateStudentRequestDTO)
        {
            if (updateStudentRequestDTO == null || updateStudentRequestDTO.Id == Guid.Empty)
                return false;

            var student = await _context.Students.FindAsync(updateStudentRequestDTO.Id);
            if (student == null || student.IsDeleted)
                return false;

            // Chỉ update khi có giá trị hợp lệ (không null hoặc không default)
            if (!string.IsNullOrEmpty(updateStudentRequestDTO.FirstName))
                student.FirstName = updateStudentRequestDTO.FirstName;

            if (!string.IsNullOrEmpty(updateStudentRequestDTO.LastName))
                student.LastName = updateStudentRequestDTO.LastName;

            // DateTime? nên check nullable, giả sử DTO dùng DateTime?
            // Nếu không nullable thì phải check giá trị khác default(DateTime)
            if (updateStudentRequestDTO.DateOfBirth != default(DateTime))
                student.DateOfBirth = updateStudentRequestDTO.DateOfBirth;

            if (!string.IsNullOrEmpty(updateStudentRequestDTO.Grade))
                student.Grade = updateStudentRequestDTO.Grade;

            if (!string.IsNullOrEmpty(updateStudentRequestDTO.Section))
                student.Section = updateStudentRequestDTO.Section;

            if (!string.IsNullOrEmpty(updateStudentRequestDTO.Image))
                student.Image = updateStudentRequestDTO.Image;

            _context.Students.Update(student);
            await _context.SaveChangesAsync();

            return true;
        }

        #region Methods for CheckupCampaign

        public async Task<List<Student>> GetStudentsByGradeAndSectionAsync(List<string> grades, List<string> sections)
        {
            var query = _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted);

            // Nếu có filter theo grade
            if (grades != null && grades.Any())
            {
                query = query.Where(s => grades.Contains(s.Grade ?? ""));
            }

            // Nếu có filter theo section
            if (sections != null && sections.Any())
            {
                query = query.Where(s => sections.Contains(s.Section ?? ""));
            }

            return await query
                .OrderBy(s => s.Grade)
                .ThenBy(s => s.Section)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<List<GetAllStudentDTO>> GetStudentsDTOByGradeAndSectionAsync(List<string> grades, List<string> sections)
        {
            var query = _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted);

            // Nếu có filter theo grade
            if (grades != null && grades.Any())
            {
                query = query.Where(s => grades.Contains(s.Grade ?? ""));
            }

            // Nếu có filter theo section
            if (sections != null && sections.Any())
            {
                query = query.Where(s => sections.Contains(s.Section ?? ""));
            }

            return await query
                .OrderBy(s => s.Grade)
                .ThenBy(s => s.Section)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new GetAllStudentDTO
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Grade = s.Grade,
                    Section = s.Section,
                    Image = s.Image
                })
                .ToListAsync();
        }

        public async Task<List<Student>> GetStudentsByIdsAsync(List<Guid> studentIds)
        {
            if (studentIds == null || !studentIds.Any())
                return new List<Student>();

            return await _context.Students
                .AsNoTracking()
                .Where(s => studentIds.Contains(s.Id) && !s.IsDeleted)
                .OrderBy(s => s.Grade)
                .ThenBy(s => s.Section)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<List<GetAllStudentDTO>> GetStudentsDTOByIdsAsync(List<Guid> studentIds)
        {
            if (studentIds == null || !studentIds.Any())
                return new List<GetAllStudentDTO>();

            return await _context.Students
                .AsNoTracking()
                .Where(s => studentIds.Contains(s.Id) && !s.IsDeleted)
                .OrderBy(s => s.Grade)
                .ThenBy(s => s.Section)
                .ThenBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Select(s => new GetAllStudentDTO
                {
                    Id = s.Id,
                    StudentCode = s.StudentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    DateOfBirth = s.DateOfBirth,
                    Grade = s.Grade,
                    Section = s.Section,
                    Image = s.Image
                })
                .ToListAsync();
        }

        public async Task<List<string>> GetAvailableGradesAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted && !string.IsNullOrEmpty(s.Grade))
                .Select(s => s.Grade!)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }

        public async Task<List<string>> GetAvailableSectionsAsync()
        {
            return await _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted && !string.IsNullOrEmpty(s.Section))
                .Select(s => s.Section!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetGradeSectionMappingAsync()
        {
            var gradeSections = await _context.Students
                .AsNoTracking()
                .Where(s => !s.IsDeleted &&
                           !string.IsNullOrEmpty(s.Grade) &&
                           !string.IsNullOrEmpty(s.Section))
                .Select(s => new { Grade = s.Grade!, Section = s.Section! })
                .Distinct()
                .ToListAsync();

            return gradeSections
                .GroupBy(gs => gs.Grade)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Section).OrderBy(s => s).ToList()
                );
        }

        #endregion
    }
}