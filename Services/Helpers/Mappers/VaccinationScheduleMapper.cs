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
            return new VaccinationScheduleDetailResponseDTO
            {
                Id = schedule.Id,
                VaccinationTypeName = schedule.VaccinationType?.Name ?? string.Empty,
                VaccinationTypeCode = schedule.VaccinationType?.Code ?? string.Empty,
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                TotalStudents = schedule.SessionStudents?.Count ?? 0,
                CompletedRecords = schedule.SessionStudents?.SelectMany(ss => ss.VaccinationRecords).Count() ?? 0,
                CampaignName = schedule.Campaign?.Name ?? string.Empty,
                SessionStudents = schedule.SessionStudents?.Select(MapToSessionStudentResponseDTO).ToList() ?? new List<SessionStudentResponseDTO>(),
                Records = schedule.SessionStudents?.SelectMany(ss => ss.VaccinationRecords).Select(MapToVaccinationRecordSummaryDTO).ToList() ?? new List<VaccinationRecordSummaryDTO>()
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
                ParentNotifiedAt = sessionStudent.ParentNotifiedAt
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