using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Implementations
{
    public class VaccinationRecordRepository : IVaccinationRecordRepository
    {
        private readonly SchoolHealthManagerDbContext _context;
        public async Task<VaccinationRecordResponse> CreateAsync(CreateVaccinationRecordRequest request)
        {
            var entity = new BusinessObjects.VaccinationRecord
            {
                Id = Guid.NewGuid(),
                CampaignId = request.CampaignId,
                StudentId = request.StudentId,
                VaccineTypeId = request.VaccineTypeId,
                VaccineLotId = request.VaccineLotId,
                AdministeredDate = request.AdministeredDate,
                ConsentSigned = request.ConsentSigned,
                VaccinatedBy = request.VaccinatedBy,
                VaccinatedAt = request.VaccinatedAt,
                ReactionFollowup24h = false,
                ReactionFollowup72h = false
            };

            _context.VaccinationRecords.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new Exception("Khởi tạo bị lỗi!");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.VaccinationRecords.FindAsync(id);
            if (entity == null) return false;

            _context.VaccinationRecords.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<VaccinationRecordResponse>> GetAllAsync()
        {
            return await _context.VaccinationRecords
                .Select(v => new VaccinationRecordResponse
                {
                    Id = v.Id,
                    CampaignId = v.CampaignId,
                    StudentId = v.StudentId,
                    VaccineTypeId = v.VaccineTypeId,
                    VaccineLotId = v.VaccineLotId,
                    AdministeredDate = v.AdministeredDate,
                    ConsentSigned = v.ConsentSigned,
                    VaccinatedBy = v.VaccinatedBy,
                    VaccinatedAt = v.VaccinatedAt,
                    ReactionFollowup24h = v.ReactionFollowup24h,
                    ReactionFollowup72h = v.ReactionFollowup72h
                }).ToListAsync();
        }

        public async Task<VaccinationRecordResponse?> GetByIdAsync(Guid id)
        {
            var record = await _context.VaccinationRecords.FindAsync(id);
            if (record == null) return null;

            return new VaccinationRecordResponse
            {
                Id = record.Id, 
                CampaignId = record.CampaignId,
                StudentId = record.StudentId,
                VaccineTypeId = record.VaccineTypeId,
                VaccineLotId = record.VaccineLotId,
                AdministeredDate = record.AdministeredDate,
                ConsentSigned = record.ConsentSigned,
                VaccinatedBy = record.VaccinatedBy,
                VaccinatedAt = record.VaccinatedAt,
                ReactionFollowup24h = record.ReactionFollowup24h,
                ReactionFollowup72h = record.ReactionFollowup72h
            };
        }

        public async Task<VaccinationRecordResponse?> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request)
        {
            var entity = await _context.VaccinationRecords.FindAsync(id);
            if (entity == null) return null;

            if (request.AdministeredDate.HasValue) entity.AdministeredDate = request.AdministeredDate.Value;
            if (request.ConsentSigned.HasValue) entity.ConsentSigned = request.ConsentSigned.Value;
            if (request.VaccineLotId.HasValue) entity.VaccineLotId = request.VaccineLotId.Value;
            if (request.VaccinatedBy.HasValue) entity.VaccinatedBy = request.VaccinatedBy.Value;
            if (request.VaccinatedAt.HasValue) entity.VaccinatedAt = request.VaccinatedAt.Value;
            if (request.ReactionFollowup24h.HasValue) entity.ReactionFollowup24h = request.ReactionFollowup24h.Value;
            if (request.ReactionFollowup72h.HasValue) entity.ReactionFollowup72h = request.ReactionFollowup72h.Value;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id);
        }
    }
}
