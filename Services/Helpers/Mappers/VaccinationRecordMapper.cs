namespace Services.Helpers.Mappers
{
    public static class VaccinationRecordMapper
    {
        public static CreateVaccinationRecordResponse MapToCreateResponseDTO(VaccinationRecord record)
        {
            return new CreateVaccinationRecordResponse
            {
                Id = record.Id,
                Message = "Vaccination record created successfully",

                // Thông tin học sinh
                StudentId = record.SessionStudent?.StudentId ?? Guid.Empty,
                StudentName = record.SessionStudent?.Student?.FullName ?? string.Empty,
                StudentCode = record.SessionStudent?.Student?.StudentCode ?? string.Empty,

                // Thông tin vắc xin: chỉ hiển thị tên loại vắc xin
                VaccineName = record.SessionStudent?.VaccinationSchedule?.VaccinationType?.Name ?? string.Empty,

                // Người tiêm
                VaccinatedById = record.VaccinatedById,
                VaccinatedBy = record.VaccinatedBy != null
                    ? $"{record.VaccinatedBy.FirstName} {record.VaccinatedBy.LastName}".Trim()
                    : string.Empty,

                AdministeredDate = record.AdministeredDate,
                ReactionFollowup24h = record.ReactionFollowup24h.ToString(),
                ReactionFollowup72h = record.ReactionFollowup72h.ToString(),
                ReactionSeverity = (int)record.ReactionSeverity,
                SessionStatus = record.SessionStudent?.Status.ToString() ?? "Unknown"
            };
        }

        public static CreateVaccinationRecordResponse MapToResponseDTO(VaccinationRecord record)
        {
            return new CreateVaccinationRecordResponse
            {
                Id = record.Id,
                Message = string.Empty,

                StudentId = record.SessionStudent?.StudentId ?? Guid.Empty,
                StudentName = record.SessionStudent?.Student?.FullName ?? string.Empty,
                StudentCode = record.SessionStudent?.Student?.StudentCode ?? string.Empty,

                VaccineName = record.SessionStudent?.VaccinationSchedule?.VaccinationType?.Name ?? string.Empty,

                VaccinatedById = record.VaccinatedById,
                VaccinatedBy = record.VaccinatedBy != null
                    ? $"{record.VaccinatedBy.FirstName} {record.VaccinatedBy.LastName}".Trim()
                    : string.Empty,

                AdministeredDate = record.AdministeredDate,
                ReactionFollowup24h = record.ReactionFollowup24h.ToString(),
                ReactionFollowup72h = record.ReactionFollowup72h.ToString(),
                ReactionSeverity = (int)record.ReactionSeverity,
                SessionStatus = record.SessionStudent?.Status.ToString() ?? "Unknown"
            };
        }

        public static CreateVaccinationRecordResponse MapToDetailResponseDTO(VaccinationRecord record)
        {
            return MapToResponseDTO(record);
        }

        public static PagedList<CreateVaccinationRecordResponse> ToPagedResponseDTO(PagedList<VaccinationRecord> source)
        {
            var items = source.Select(MapToResponseDTO).ToList();
            return new PagedList<CreateVaccinationRecordResponse>(
                items,
                source.MetaData.TotalCount,
                source.MetaData.CurrentPage,
                source.MetaData.PageSize
            );
        }
    }
}
