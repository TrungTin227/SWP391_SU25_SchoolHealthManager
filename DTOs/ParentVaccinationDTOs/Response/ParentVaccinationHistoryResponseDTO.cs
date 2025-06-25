using BusinessObjects.Common;

namespace DTOs.ParentVaccinationDTOs.Response
{
    public class ParentVaccinationHistoryResponseDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public List<VaccinationHistoryRecordDTO> VaccinationHistory { get; set; } = new List<VaccinationHistoryRecordDTO>();
    }

    public class VaccinationHistoryRecordDTO
    {
        public Guid RecordId { get; set; }
        public string VaccineName { get; set; } = string.Empty;
        public string CampaignName { get; set; } = string.Empty;
        public DateTime VaccinatedAt { get; set; }
        public string VaccinatedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }

        // Thông tin phản ứng
        public VaccinationReactionSeverity ReactionSeverity { get; set; }
        public string? ReactionNotes { get; set; }
        public DateTime? ReactionReportedAt { get; set; }

        // Thông tin vaccine
        public string? VaccineLot { get; set; }
        public DateTime? VaccineExpiryDate { get; set; }
        public string? Manufacturer { get; set; }
    }
}