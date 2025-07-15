using DTOs.ParentVaccinationDTOs.Response;

namespace Services.Helpers.Mappers
{
    public static class ParentVaccinationMapper
    {
        public static ParentVaccinationScheduleResponseDTO MapToScheduleResponseDTO(
            VaccinationSchedule schedule, Guid parentUserId)
        {
            var parentSessions = schedule.SessionStudents
                .Where(ss => ss.Student.ParentUserId == parentUserId)
                .ToList();

            var actionStatus = DetermineActionStatus(schedule, parentSessions);

            return new ParentVaccinationScheduleResponseDTO
            {
                Id = schedule.Id,
                CampaignName = schedule.Campaign?.Name ?? "",
                VaccinationTypeName = schedule.VaccinationType?.Name ?? "",
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                ActionStatus = actionStatus,
                ConsentDeadline = parentSessions.FirstOrDefault()?.ConsentDeadline,
                Students = parentSessions.Select(MapToStudentVaccinationDTO).ToList(),
                PendingConsentCount = parentSessions.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Pending || 
                    ss.ConsentStatus == ParentConsentStatus.Sent),
                ApprovedConsentCount = parentSessions.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Approved),
                RejectedConsentCount = parentSessions.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Rejected)
            };
        }

        public static ParentVaccinationScheduleResponseDTO MapToScheduleDetailResponseDTO(
            VaccinationSchedule schedule, List<SessionStudent> sessionStudents)
        {
            var actionStatus = DetermineActionStatus(schedule, sessionStudents);

            return new ParentVaccinationScheduleResponseDTO
            {
                Id = schedule.Id,
                CampaignName = schedule.Campaign?.Name ?? "",
                VaccinationTypeName = schedule.VaccinationType?.Name ?? "",
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                ActionStatus = actionStatus,
                ConsentDeadline = sessionStudents.FirstOrDefault()?.ConsentDeadline,
                Students = sessionStudents.Select(MapToStudentVaccinationDTO).ToList(),
                PendingConsentCount = sessionStudents.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Pending || 
                    ss.ConsentStatus == ParentConsentStatus.Sent),
                ApprovedConsentCount = sessionStudents.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Approved),
                RejectedConsentCount = sessionStudents.Count(ss => 
                    ss.ConsentStatus == ParentConsentStatus.Rejected)
            };
        }

        public static ParentStudentVaccinationDTO MapToStudentVaccinationDTO(SessionStudent sessionStudent)
        {
            var vaccinationRecord = sessionStudent.VaccinationRecords.FirstOrDefault();
            
            return new ParentStudentVaccinationDTO
            {
                StudentId = sessionStudent.StudentId,
                StudentName = sessionStudent.Student?.FullName ?? "",
                StudentCode = sessionStudent.Student?.StudentCode ?? "",
                Grade = sessionStudent.Student?.Grade ?? "",
                Section = sessionStudent.Student?.Section ?? "",
                SessionStudentId = sessionStudent.Id,
                ConsentStatus = sessionStudent.ConsentStatus,
                ParentSignedAt = sessionStudent.ParentSignedAt,
                ParentNotes = sessionStudent.ParentNotes,
                ConsentDeadline = sessionStudent.ConsentDeadline,
                IsVaccinated = vaccinationRecord != null,
                VaccinatedAt = vaccinationRecord?.VaccinatedAt,
                ReactionSeverity = vaccinationRecord?.ReactionSeverity,
                RequiresFollowUp = vaccinationRecord?.ReactionSeverity > VaccinationReactionSeverity.None
            };
        }

        public static VaccinationHistoryRecordDTO MapToHistoryRecordDTO(VaccinationRecord record)
        {
            return new VaccinationHistoryRecordDTO
            {
                RecordId = record.Id,
                VaccineName = record.SessionStudent.VaccinationSchedule.VaccinationType?.Name ?? "", 
                CampaignName = record.SessionStudent.VaccinationSchedule.Campaign?.Name ?? "", 
                VaccinatedAt = record.VaccinatedAt,
                VaccinatedBy = record.VaccinatedBy?.FullName ?? "",
                ReactionSeverity = record.ReactionSeverity,
                //VaccineLot = record.VaccineLot?.LotNumber ?? "",
                //VaccineExpiryDate = record.VaccineLot?.ExpiryDate,
            };
        }

        private static ParentActionStatus DetermineActionStatus(
            VaccinationSchedule schedule, List<SessionStudent> sessions)
        {
            if (!sessions.Any()) return ParentActionStatus.PendingConsent;

            var statuses = sessions.Select(ss => ss.ConsentStatus).Distinct().ToList();
            
            if (statuses.Count > 1) return ParentActionStatus.Mixed;

            var status = statuses.First();
            var hasFollowUp = sessions.Any(ss => 
                ss.VaccinationRecords.Any(vr => vr.ReactionSeverity > VaccinationReactionSeverity.None));

            return status switch
            {
                ParentConsentStatus.Pending or ParentConsentStatus.Sent => ParentActionStatus.PendingConsent,
                ParentConsentStatus.Approved when schedule.ScheduleStatus == ScheduleStatus.Completed => 
                    hasFollowUp ? ParentActionStatus.RequiresFollowUp : ParentActionStatus.Completed,
                ParentConsentStatus.Approved => ParentActionStatus.Approved,
                _ => ParentActionStatus.Completed
            };
        }

        public static ParentVaccinationRespondDTO ToDTO(ParentVaccinationRecord entity)
        {
            if (entity == null) return null;

            return new ParentVaccinationRespondDTO
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                ParentUserId = entity.ParentUserId,
                VaccineTypeId = entity.VaccineTypeId,
                DoseNumber = entity.DoseNumber,
                AdministeredAt = entity.AdministeredAt
            };
        }

        public static List<ParentVaccinationRespondDTO> ToDTOList(List<ParentVaccinationRecord> entities)
        {
            if (entities == null) return new List<ParentVaccinationRespondDTO>();

            return entities.Select(ToDTO).ToList();
        }
    }
}