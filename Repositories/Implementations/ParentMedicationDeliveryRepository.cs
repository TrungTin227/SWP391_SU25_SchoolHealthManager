using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.ParentMedicationDeliveryDTOs.Request;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using DTOs.StudentDTOs.Response;
using Microsoft.EntityFrameworkCore;
using Repositories.WorkSeeds.Implements;

namespace Repositories.Implementations
{
    public class ParentMedicationDeliveryRepository : GenericRepository<ParentMedicationDelivery, Guid>, IParentMedicationDeliveryRepository
    {

        public ParentMedicationDeliveryRepository(SchoolHealthManagerDbContext context) : base(context)
        {
        }

        public async Task<CreateParentMedicationDeliveryRequestDTO> CreateParentMedicationDeliveryRequestDTO(CreateParentMedicationDeliveryRequestDTO request)
        {
            var parentmedicationDelivery = new ParentMedicationDelivery
            {
                StudentId = request.StudentId,
                ParentId = request.ParentId,
                QuantityDelivered = request.QuantityDelivered,
                ReceivedBy = request.ParentId, // Sửa: Thêm trường ReceivedBy
                DeliveredAt = request.DeliveredAt,
                Notes = request.Notes,
                Status = request.Status
            };
            await _context.AddAsync(parentmedicationDelivery);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<List<GetParentMedicationDeliveryRespondDTO>> GetAllParentMedicationDeliveryByParentIdDTO(Guid id)
        {
            return await _context.ParentMedicationDeliveries
                   .Where(s => !s.IsDeleted && s.ParentId == id)
                   .OrderBy(s => s.CreatedAt)
                   .Select(s => new GetParentMedicationDeliveryRespondDTO
                   {
                       ParentMedicationDeliveryId = s.Id,
                       ParentId = s.ParentId,
                       StudentId = s.StudentId,
                       ReceivedBy = s.ReceivedBy,
                       QuantityDelivered = s.QuantityDelivered,
                       DeliveredAt = s.DeliveredAt,
                       Notes = s.Notes,
                       Status = s.Status.ToString(),
                   })

                   .ToListAsync();
        }

        public async Task<List<GetParentMedicationDeliveryRespondDTO>> GetAllParentMedicationDeliveryDTO()
        {
            return await _context.ParentMedicationDeliveries
                   .Where(s => !s.IsDeleted)
                   .OrderBy(s => s.CreatedAt)
                   .Select(s => new GetParentMedicationDeliveryRespondDTO
                   {
                       ParentMedicationDeliveryId = s.Id,
                       ParentId = s.ParentId,
                       StudentId = s.StudentId,
                       ReceivedBy = s.ReceivedBy,
                       QuantityDelivered = s.QuantityDelivered,
                       DeliveredAt = s.DeliveredAt,
                       Notes = s.Notes,
                       Status = s.Status.ToString(),
                   }).ToListAsync();
        }

        public async Task<GetParentMedicationDeliveryRespondDTO?> GetParentMedicationDeliveryByIdDTO(Guid id)
        {
            return await _context.ParentMedicationDeliveries
                .Where(s => !s.IsDeleted && s.Id == id)
                .Select(s => new GetParentMedicationDeliveryRespondDTO
                {
                    ParentMedicationDeliveryId = s.Id,
                    ParentId = s.ParentId,
                    StudentId = s.StudentId,
                    ReceivedBy = s.ReceivedBy,
                    QuantityDelivered = s.QuantityDelivered,
                    DeliveredAt = s.DeliveredAt,
                    Notes = s.Notes,
                    Status = s.Status.ToString(),
                })
                .FirstOrDefaultAsync(); // Lúc này mới đúng nghĩa là lấy 1 cái DTO ra
        }

    }
}
