namespace Services.Helpers.Mappers
{
    public static class VaccinationScheduleMapper
    {
        public static VaccinationScheduleResponseDTO MapToResponseDTO(VaccinationSchedule schedule)
        {
            return new VaccinationScheduleResponseDTO
            {
                Id = schedule.Id,
                VaccinationTypeName = schedule.VaccinationType?.Name ?? string.Empty,
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                TotalStudents = schedule.SessionStudents?.Count ?? 0,
                CompletedRecords = schedule.SessionStudents?.SelectMany(ss => ss.VaccinationRecords).Count() ?? 0 
            };
        }

        public static VaccinationScheduleDetailResponseDTO MapToDetailResponseDTO(VaccinationSchedule schedule)
        {
            var sessionStudents = schedule.SessionStudents?.ToList() ?? new List<SessionStudent>();

            return new VaccinationScheduleDetailResponseDTO
            {
                Id = schedule.Id,
                VaccinationTypeName = schedule.VaccinationType?.Name ?? string.Empty,
                VaccinationTypeCode = schedule.VaccinationType?.Code ?? string.Empty,
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                TotalStudents = sessionStudents.Count,
                CompletedRecords = sessionStudents.SelectMany(ss => ss.VaccinationRecords).Count(),
                CampaignName = schedule.Campaign?.Name ?? string.Empty,

                // Tính thống kê consent
                PendingConsentCount = sessionStudents.Count(ss =>
                    ss.ConsentStatus == ParentConsentStatus.Pending ||
                    ss.ConsentStatus == ParentConsentStatus.Sent),
                ApprovedConsentCount = sessionStudents.Count(ss =>
                    ss.ConsentStatus == ParentConsentStatus.Approved),
                RejectedConsentCount = sessionStudents.Count(ss =>
                    ss.ConsentStatus == ParentConsentStatus.Rejected),

                // Chỉ 1 trường vaccine dự kiến = số học sinh đã đồng ý 
                VaccineExpectedCount = sessionStudents.Count(ss =>
                    ss.ConsentStatus == ParentConsentStatus.Approved),

                SessionStudents = sessionStudents.Select(MapToSessionStudentResponseDTO).ToList(),
                Records = sessionStudents.SelectMany(ss => ss.VaccinationRecords)
                    .Select(MapToVaccinationRecordSummaryDTO).ToList()
            };
        }

        public static SessionStudentResponseDTO MapToSessionStudentResponseDTO(SessionStudent sessionStudent)
        {
            return new SessionStudentResponseDTO
            {
                Id = sessionStudent.Id,
                StudentId = sessionStudent.StudentId,
                StudentName = sessionStudent.Student?.FullName ?? string.Empty,
                StudentCode = sessionStudent.Student?.StudentCode ?? string.Empty,
                Status = sessionStudent.Status,
                CheckInTime = sessionStudent.CheckInTime,
                Notes = sessionStudent.Notes,
                ParentNotifiedAt = sessionStudent.ParentNotifiedAt,
                // Thêm các trường consent
            ConsentStatus = sessionStudent.ConsentStatus,
                ParentSignedAt = sessionStudent.ParentSignedAt,
                ParentNotes = sessionStudent.ParentNotes,
                ParentSignature = sessionStudent.ParentSignature,
                ConsentDeadline = sessionStudent.ConsentDeadline
            };
        }

        public static VaccinationRecordSummaryDTO MapToVaccinationRecordSummaryDTO(VaccinationRecord record)
        {
            return new VaccinationRecordSummaryDTO
            {
                Id = record.Id,
                StudentName = record.Student?.FullName ?? string.Empty,
                AdministeredDate = record.AdministeredDate,
                VaccinatedByName = record.VaccinatedBy != null
                    ? $"{record.VaccinatedBy.FirstName} {record.VaccinatedBy.LastName}".Trim()
                    : string.Empty,
                ReactionFollowup24h = record.ReactionFollowup24h,
                ReactionFollowup72h = record.ReactionFollowup72h
            };
        }

        public static PagedList<VaccinationScheduleResponseDTO> ToPagedResponseDTO(PagedList<VaccinationSchedule> source)
        {
            var items = source.Select(MapToResponseDTO).ToList();
            return new PagedList<VaccinationScheduleResponseDTO>(
                items,
                source.MetaData.TotalCount,
                source.MetaData.CurrentPage,
                source.MetaData.PageSize);
        }
        public static VaccinationScheduleForParentResponseDTO MapToParentDTO(SessionStudent ss)
        {
            var schedule = ss.VaccinationSchedule; // cần Include Navigation
            return new VaccinationScheduleForParentResponseDTO
            {
                Id                 = schedule.Id,
                VaccinationTypeName = schedule.VaccinationType?.Name ?? string.Empty,
                ScheduledAt        = schedule.ScheduledAt,
                ScheduleStatus     = schedule.ScheduleStatus,
                StudentName        = ss.Student?.FullName ?? string.Empty,
                ConsentStatus = ss.ConsentStatus, // thêm
            };
        }

    }
}