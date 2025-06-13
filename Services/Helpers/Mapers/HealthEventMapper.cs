using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;

namespace Services.Mappers
{
    public static class HealthEventMapper
    {
        public static HealthEvent MapFromCreateRequest(CreateHealthEventRequestDTO request)
        {
            return new HealthEvent
            {
                StudentId = request.StudentId,
                EventCategory = request.EventCategory,
                VaccinationRecordId = request.VaccinationRecordId,
                EventType = request.EventType,
                Description = request.Description,
                OccurredAt = request.OccurredAt
            };
        }

        public static HealthEventResponseDTO MapToResponseDTO(HealthEvent healthEvent)
        {
            return new HealthEventResponseDTO
            {
                Id = healthEvent.Id,
                StudentId = healthEvent.StudentId,
                StudentName = healthEvent.Student?.FullName ?? string.Empty,
                EventCategory = healthEvent.EventCategory.ToString(),
                VaccinationRecordId = healthEvent.VaccinationRecordId,
                EventType = healthEvent.EventType.ToString(),
                Description = healthEvent.Description,
                OccurredAt = healthEvent.OccurredAt,
                EventStatus = healthEvent.EventStatus.ToString(),
                ReportedBy = healthEvent.ReportedUserId,
                ReportedByName = healthEvent.ReportedUser?.FullName ?? string.Empty,
                CreatedAt = healthEvent.CreatedAt,
                UpdatedAt = healthEvent.UpdatedAt,
                IsDeleted = healthEvent.IsDeleted,
                TotalMedications = healthEvent.EventMedications?.Count ?? 0,
                TotalSupplies = healthEvent.SupplyUsages?.Count ?? 0
            };
        }

        public static HealthEventDetailResponseDTO MapToDetailResponseDTO(HealthEvent healthEvent)
        {
            var baseDto = MapToResponseDTO(healthEvent);

            return new HealthEventDetailResponseDTO
            {
                Id = baseDto.Id,
                StudentId = baseDto.StudentId,
                StudentName = baseDto.StudentName,
                EventCategory = baseDto.EventCategory,
                VaccinationRecordId = baseDto.VaccinationRecordId,
                EventType = baseDto.EventType,
                Description = baseDto.Description,
                OccurredAt = baseDto.OccurredAt,
                EventStatus = baseDto.EventStatus,
                ReportedBy = baseDto.ReportedBy,
                ReportedByName = baseDto.ReportedByName,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,
                TotalMedications = baseDto.TotalMedications,
                TotalSupplies = baseDto.TotalSupplies,

                Medications = healthEvent.EventMedications?.Select(MapToEventMedicationResponseDTO).ToList()
                    ?? new List<EventMedicationResponseDTO>(),

                Supplies = healthEvent.SupplyUsages?.Select(MapToSupplyUsageResponseDTO).ToList()
                    ?? new List<SupplyUsageResponseDTO>()
            };
        }

        public static EventMedicationResponseDTO MapToEventMedicationResponseDTO(EventMedication eventMedication)
        {
            return new EventMedicationResponseDTO
            {
                Id = eventMedication.Id,
                MedicationLotId = eventMedication.MedicationLotId,
                MedicationName = eventMedication.MedicationLot?.Medication?.Name ?? string.Empty,
                LotNumber = eventMedication.MedicationLot?.LotNumber ?? string.Empty,
                Quantity = eventMedication.Quantity,
                UsedAt = eventMedication.CreatedAt
            };
        }

        public static SupplyUsageResponseDTO MapToSupplyUsageResponseDTO(SupplyUsage supplyUsage)
        {
            return new SupplyUsageResponseDTO
            {
                Id = supplyUsage.Id,
                HealthEventId = supplyUsage.HealthEventId,
                MedicalSupplyLotId = supplyUsage.MedicalSupplyLotId,
                MedicalSupplyName = supplyUsage.MedicalSupplyLot?.MedicalSupply?.Name ?? string.Empty,
                LotNumber = supplyUsage.MedicalSupplyLot?.LotNumber ?? string.Empty,
                QuantityUsed = supplyUsage.QuantityUsed,
                NurseProfileId = supplyUsage.NurseProfileId,
                NurseName = supplyUsage.UsedByNurse?.User?.FullName ?? string.Empty,
                Notes = supplyUsage.Notes,
                CreatedAt = supplyUsage.CreatedAt
            };
        }

        public static PagedList<HealthEventResponseDTO> CreatePagedResult(
            PagedList<HealthEvent> sourcePaged,
            List<HealthEventResponseDTO> mappedItems)
        {
            return new PagedList<HealthEventResponseDTO>(
                mappedItems,
                sourcePaged.MetaData.TotalCount,
                sourcePaged.MetaData.CurrentPage,
                sourcePaged.MetaData.PageSize);
        }

        // Extension methods for easier usage
        public static List<HealthEventResponseDTO> ToResponseDTOList(this IEnumerable<HealthEvent> healthEvents)
        {
            return healthEvents.Select(MapToResponseDTO).ToList();
        }

        public static PagedList<HealthEventResponseDTO> ToPagedResponseDTO(this PagedList<HealthEvent> pagedHealthEvents)
        {
            var mappedItems = pagedHealthEvents.ToResponseDTOList();
            return CreatePagedResult(pagedHealthEvents, mappedItems);
        }
    }
}