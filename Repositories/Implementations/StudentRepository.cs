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
        private static readonly Guid SystemGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

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
    }
}
