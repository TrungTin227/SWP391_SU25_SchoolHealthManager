using DTOs.VaccinationRecordDTOs.Response;

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

                StudentId = record.StudentId,
                StudentName = record.Student?.FullName ?? string.Empty,
                StudentCode = record.Student?.StudentCode ?? string.Empty,

                // Thông tin vắc xin
                VaccineName = record.VaccineLot?.Medication?.Name ?? string.Empty,
                LotNumber = record.VaccineLot?.LotNumber ?? string.Empty,
                ExpirationDate = record.VaccineLot?.ExpiryDate,

                // Thông tin người tiêm
                VaccinatedBy = record.VaccinatedBy != null
                    ? $"{record.VaccinatedBy.FirstName} {record.VaccinatedBy.LastName}".Trim()
                    : string.Empty,

                AdministeredDate = record.AdministeredDate,
                ReactionFollowup24h = record.ReactionFollowup24h.ToString(),
                ReactionFollowup72h = record.ReactionFollowup72h.ToString(),

                Notes = null, // Nếu có trường Notes thì bổ sung
                Status = record.ReactionSeverity.ToString()
            };
        }

        public static CreateVaccinationRecordResponse MapToResponseDTO(VaccinationRecord record)
        {
            return new CreateVaccinationRecordResponse
            {
                Id = record.Id,
                Message = string.Empty, // không cần thông báo khi chỉ xem
                StudentId = record.StudentId,
                StudentName = record.Student?.FullName ?? string.Empty,
                StudentCode = record.Student?.StudentCode ?? string.Empty,

                VaccineName = record.VaccineLot?.Medication?.Name ?? string.Empty,
                LotNumber = record.VaccineLot?.LotNumber ?? string.Empty,
                ExpirationDate = record.VaccineLot?.ExpiryDate,

                VaccinatedBy = record.VaccinatedBy != null
                    ? $"{record.VaccinatedBy.FirstName} {record.VaccinatedBy.LastName}".Trim()
                    : string.Empty,

                AdministeredDate = record.AdministeredDate,
                ReactionFollowup24h = record.ReactionFollowup24h.ToString(),
                ReactionFollowup72h = record.ReactionFollowup72h.ToString(),

                Notes = null,
                Status = record.ReactionSeverity.ToString()
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
