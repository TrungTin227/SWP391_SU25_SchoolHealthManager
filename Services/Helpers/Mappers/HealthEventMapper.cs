namespace Services.Helpers.Mappers
{
    public static class HealthEventMapper
    {
        public static HealthEvent MapFromCreateRequest(CreateHealthEventRequestDTO dto)
        {
            return new HealthEvent
            {
                StudentId = dto.StudentId,
                EventCategory = dto.EventCategory,
                VaccinationRecordId = dto.VaccinationRecordId,
                EventType = dto.EventType,
                Description = dto.Description,
                OccurredAt = dto.OccurredAt,

                // Các thông tin ban đầu
                Location = dto.Location,
                InjuredBodyPartsRaw = dto.InjuredBodyPartsRaw,
                Severity = dto.Severity,
                Symptoms = dto.Symptoms,
            };
        }
        /// <summary>
        /// Map từ HealthEvent → HealthEventResponseDTO (bản response cơ bản)
        /// </summary>
        public static HealthEventResponseDTO MapToResponseDTO(HealthEvent he)
        {
            return new HealthEventResponseDTO
            {
                Id = he.Id,
                EventCode = he.EventCode,
                StudentId = he.StudentId,
                StudentName = $"{he.Student?.FirstName} {he.Student?.LastName}".Trim(),
                EventCategory = he.EventCategory.ToString(),
                EventType = he.EventType.ToString(),
                Description = he.Description,
                OccurredAt = he.OccurredAt,
                EventStatus = he.EventStatus.ToString(),
                ReportedBy = he.ReportedUserId,
                ReportedByName = $"{he.ReportedUser?.FirstName} {he.ReportedUser?.LastName}".Trim(),
                CreatedAt = he.CreatedAt,
                UpdatedAt = he.UpdatedAt,
                IsDeleted = he.IsDeleted,

                // Thông tin hiện có
                Location = he.Location,
                InjuredBodyPartsRaw = he.InjuredBodyPartsRaw,
                Severity = he.Severity?.ToString(),
                Symptoms = he.Symptoms,

                // Các trường sẽ có giá trị sau bước treat
                FirstAidAt = he.FirstAidAt,
                FirstResponderName = he.FirstResponder?.User?.FullName,
                FirstAidDescription = he.FirstAidDescription,
                ResolvedAt = he.ResolvedAt,
                ParentAcknowledgmentStatus = he.ParentAckStatus,
                ParentAcknowledgedAt = he.ParentAcknowledgedAt,
                ParentNotifiedAt = he.ParentNotifiedAt,
                IsReferredToHospital = he.IsReferredToHospital,
                ReferralHospital = he.ReferralHospital,
                AdditionalNotes = he.AdditionalNotes,
                AttachmentUrlsRaw = he.AttachmentUrlsRaw,
                WitnessesRaw = he.WitnessesRaw,

                TotalMedications = he.EventMedications?.Count ?? 0,
                TotalSupplies = he.SupplyUsages?.Count ?? 0
            };
        }

        /// <summary>
        /// Map chi tiết (đã có đầy đủ)
        /// </summary>
        public static HealthEventDetailResponseDTO MapToDetailResponseDTO(HealthEvent he)
        {
            // Tạo một đối tượng DTO chi tiết ngay từ đầu
            var dto = new HealthEventDetailResponseDTO
            {
                // --- Phần 1: Gán TẤT CẢ các trường được kế thừa từ HealthEventResponseDTO ---

                // Identification
                Id = he.Id,
                EventCode = he.EventCode,

                // Student
                StudentId = he.StudentId,
                StudentName = $"{he.Student?.FirstName} {he.Student?.LastName}".Trim(),

                // Event core
                EventCategory = he.EventCategory.ToString(),
                EventType = he.EventType.ToString(),
                Description = he.Description,
                OccurredAt = he.OccurredAt,
                EventStatus = he.EventStatus.ToString(),

                // Optional vaccination link
                VaccinationRecordId = he.VaccinationRecordId,

                // Location & injury details
                Location = he.Location,
                InjuredBodyPartsRaw = he.InjuredBodyPartsRaw,
                Severity = he.Severity?.ToString(),
                Symptoms = he.Symptoms,

                // First aid (phần cơ bản)
                FirstAidAt = he.FirstAidAt,
                FirstResponderName = he.FirstResponder?.User?.FullName, // Đã sửa để an toàn với null

                // Parent notification (phần cơ bản)
                ParentNotifiedAt = he.ParentNotifiedAt,
                ParentAcknowledgmentStatus = he.ParentAckStatus,
                ParentAcknowledgedAt = he.ParentAcknowledgedAt,

                // Referral (phần cơ bản)
                IsReferredToHospital = he.IsReferredToHospital,
                ReferralHospital = he.ReferralHospital,
                ResolvedAt = he.ResolvedAt,

                // Audit
                ReportedBy = he.ReportedUserId,
                ReportedByName = $"{he.ReportedUser?.FirstName} {he.ReportedUser?.LastName}".Trim(),
                CreatedAt = he.CreatedAt,
                UpdatedAt = he.UpdatedAt,
                IsDeleted = he.IsDeleted,

                // Statistics (phần này thực chất là tính toán, không phải kế thừa trực tiếp)
                TotalMedications = he.EventMedications?.Count ?? 0,
                TotalSupplies = he.SupplyUsages?.Count ?? 0,

                // --- Phần 2: Gán các trường CHỈ CÓ trong HealthEventDetailResponseDTO ---

                // Extended first-aid info
                FirstAidDescription = he.FirstAidDescription,

                // Extended parent notification
                ParentNotificationMethod = he.ParentNotificationMethod,
                ParentNotificationNote = he.ParentNotificationNote,

                // Extended referral
                ReferralDepartureTime = he.ReferralDepartureTime,
                ReferralTransportBy = he.ReferralTransportBy,

                // Parent arrival
                ParentSignatureUrl = he.ParentSignatureUrl,
                ParentArrivalAt = he.ParentArrivalAt,
                ParentReceivedBy = he.ParentReceivedBy,

                // Extra notes & evidence
                AdditionalNotes = he.AdditionalNotes,
                AttachmentUrlsRaw = he.AttachmentUrlsRaw,
                WitnessesRaw = he.WitnessesRaw,

                // Detailed lists
                Medications = he.EventMedications?.Select(MapToEventMedicationResponseDTO).ToList() ?? new List<EventMedicationResponseDTO>(),
                Supplies = he.SupplyUsages?.Select(MapToSupplyUsageResponseDTO).ToList() ?? new List<SupplyUsageResponseDTO>()
            };

            return dto;
        }
        //public static HealthEvent MapFromCreateRequest(CreateHealthEventRequestDTO request)
        //{
        //    return new HealthEvent
        //    {
        //        StudentId = request.StudentId,
        //        EventCategory = request.EventCategory,
        //        VaccinationRecordId = request.VaccinationRecordId,
        //        EventType = request.EventType,
        //        Description = request.Description,
        //        OccurredAt = request.OccurredAt
        //    };
        //}

        //public static HealthEventResponseDTO MapToResponseDTO(HealthEvent healthEvent)
        //{
        //    return new HealthEventResponseDTO
        //    {
        //        Id = healthEvent.Id,
        //        StudentId = healthEvent.StudentId,
        //        StudentName = healthEvent.Student?.FullName ?? string.Empty,
        //        EventCategory = healthEvent.EventCategory.ToString(),
        //        VaccinationRecordId = healthEvent.VaccinationRecordId,
        //        EventType = healthEvent.EventType.ToString(),
        //        Description = healthEvent.Description,
        //        OccurredAt = healthEvent.OccurredAt,
        //        EventStatus = healthEvent.EventStatus.ToString(),
        //        ReportedBy = healthEvent.ReportedUserId,
        //        ReportedByName = healthEvent.ReportedUser?.FullName ?? string.Empty,
        //        CreatedAt = healthEvent.CreatedAt,
        //        UpdatedAt = healthEvent.UpdatedAt,
        //        IsDeleted = healthEvent.IsDeleted,
        //        TotalMedications = healthEvent.EventMedications?.Count ?? 0,
        //        TotalSupplies = healthEvent.SupplyUsages?.Count ?? 0
        //    };
        //}

        //public static HealthEventDetailResponseDTO MapToDetailResponseDTO(HealthEvent healthEvent)
        //{
        //    var baseDto = MapToResponseDTO(healthEvent);

        //    return new HealthEventDetailResponseDTO
        //    {
        //        Id = baseDto.Id,
        //        StudentId = baseDto.StudentId,
        //        StudentName = baseDto.StudentName,
        //        EventCategory = baseDto.EventCategory,
        //        VaccinationRecordId = baseDto.VaccinationRecordId,
        //        EventType = baseDto.EventType,
        //        Description = baseDto.Description,
        //        OccurredAt = baseDto.OccurredAt,
        //        EventStatus = baseDto.EventStatus,
        //        ReportedBy = baseDto.ReportedBy,
        //        ReportedByName = baseDto.ReportedByName,
        //        CreatedAt = baseDto.CreatedAt,
        //        UpdatedAt = baseDto.UpdatedAt,
        //        IsDeleted = baseDto.IsDeleted,
        //        TotalMedications = baseDto.TotalMedications,
        //        TotalSupplies = baseDto.TotalSupplies,

        //        Medications = healthEvent.EventMedications?.Select(MapToEventMedicationResponseDTO).ToList()
        //            ?? new List<EventMedicationResponseDTO>(),

        //        Supplies = healthEvent.SupplyUsages?.Select(MapToSupplyUsageResponseDTO).ToList()
        //            ?? new List<SupplyUsageResponseDTO>()
        //    };
        //}

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