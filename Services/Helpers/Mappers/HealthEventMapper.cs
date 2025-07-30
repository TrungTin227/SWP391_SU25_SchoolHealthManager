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

                // ---- thêm mới -----
                Location = dto.Location,
                InjuredBodyPartsRaw = dto.InjuredBodyPartsRaw,
                Severity = dto.Severity,
                Symptoms = dto.Symptoms,
                FirstAidAt = dto.FirstAidAt,
                FirstResponderId = dto.FirstResponderId,
                FirstAidDescription = dto.FirstAidDescription,
                ParentNotifiedAt = dto.ParentNotifiedAt,
                ParentNotificationMethod = dto.ParentNotificationMethod,
                ParentNotificationNote = dto.ParentNotificationNote,
                IsReferredToHospital = dto.IsReferredToHospital,
                ReferralHospital = dto.ReferralHospital,
                ReferralDepartureTime = dto.ReferralDepartureTime,
                ReferralTransportBy = dto.ReferralTransportBy,
                ParentSignatureUrl = dto.ParentSignatureUrl,
                AdditionalNotes = dto.AdditionalNotes,
                AttachmentUrlsRaw = dto.AttachmentUrlsRaw,
                WitnessesRaw = dto.WitnessesRaw
            };
        }
        public static HealthEventResponseDTO MapToResponseDTO(HealthEvent he)
        {
            return new HealthEventResponseDTO
            {
                Id = he.Id,
                StudentId = he.StudentId,
                StudentName = (he.Student?.FirstName ?? "") + " " + (he.Student?.LastName ?? ""),
                EventCategory = he.EventCategory.ToString(),
                VaccinationRecordId = he.VaccinationRecordId,
                EventType = he.EventType.ToString(),
                Description = he.Description,
                OccurredAt = he.OccurredAt,
                EventStatus = he.EventStatus.ToString(),
                ReportedBy = he.ReportedUserId,
                ReportedByName = (he.ReportedUser?.FirstName ?? "") + " " + (he.ReportedUser?.LastName ?? ""),
                CreatedAt = he.CreatedAt,
                UpdatedAt = he.UpdatedAt,
                IsDeleted = he.IsDeleted,
                EventCode = he.EventCode,
                Location = he.Location,
                Severity = he.Severity?.ToString(),
                ResolvedAt = he.ResolvedAt,
                TotalMedications = he.EventMedications?.Count ?? 0,
                TotalSupplies = he.SupplyUsages?.Count ?? 0
            };
        }

        // 3. Map chi tiết
        public static HealthEventDetailResponseDTO MapToDetailResponseDTO(HealthEvent he)
        {
            // Bước 1: Gọi phương thức map cơ bản để lấy các thông tin chung
            var baseDto = MapToResponseDTO(he);

            // Bước 2: Tạo đối tượng chi tiết và gán các giá trị từ DTO cơ bản
            // và bổ sung thêm các trường chi tiết.
            return new HealthEventDetailResponseDTO
            {
                // Gán lại toàn bộ các trường từ DTO cơ bản
                Id = baseDto.Id,
                EventCode = baseDto.EventCode,
                StudentId = baseDto.StudentId,
                StudentName = baseDto.StudentName,
                EventCategory = baseDto.EventCategory,
                VaccinationRecordId = baseDto.VaccinationRecordId,
                EventType = baseDto.EventType,
                Description = baseDto.Description,
                OccurredAt = baseDto.OccurredAt,
                EventStatus = baseDto.EventStatus,
                Location = baseDto.Location,
                Severity = baseDto.Severity,
                ResolvedAt = baseDto.ResolvedAt,
                ReportedBy = baseDto.ReportedBy,
                ReportedByName = baseDto.ReportedByName,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,
                TotalMedications = baseDto.TotalMedications,
                TotalSupplies = baseDto.TotalSupplies,

                // Bổ sung các trường chỉ có trong DTO chi tiết
                InjuredBodyPartsRaw = he.InjuredBodyPartsRaw,
                Symptoms = he.Symptoms,
                FirstAidAt = he.FirstAidAt,
                FirstResponderName = he.FirstResponder?.User.FullName, // Ví dụ: bạn có thể muốn lấy tên người sơ cứu
                FirstAidDescription = he.FirstAidDescription,
                ParentNotifiedAt = he.ParentNotifiedAt,
                ParentNotificationMethod = he.ParentNotificationMethod,
                ParentNotificationNote = he.ParentNotificationNote,
                IsReferredToHospital = he.IsReferredToHospital,
                ReferralHospital = he.ReferralHospital,
                ReferralDepartureTime = he.ReferralDepartureTime,
                ReferralTransportBy = he.ReferralTransportBy,
                ParentSignatureUrl = he.ParentSignatureUrl,
                ParentArrivalAt = he.ParentArrivalAt,
                ParentReceivedBy = he.ParentReceivedBy,
                AdditionalNotes = he.AdditionalNotes,
                AttachmentUrlsRaw = he.AttachmentUrlsRaw,
                WitnessesRaw = he.WitnessesRaw,

                // Bổ sung các danh sách chi tiết
                Medications = he.EventMedications?.Select(MapToEventMedicationResponseDTO).ToList() ?? new(),
                Supplies = he.SupplyUsages?.Select(MapToSupplyUsageResponseDTO).ToList() ?? new()
            };
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