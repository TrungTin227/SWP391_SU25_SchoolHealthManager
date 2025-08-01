using BusinessObjects.Common;

namespace DTOs.VaccinationScheduleDTOs.Response
{
    public class SessionStudentResponseDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public SessionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? CheckInTime { get; set; }
        public string? Notes { get; set; }
        public DateTime? ParentNotifiedAt { get; set; }
        // Thêm các trường consent
        public ParentConsentStatus ConsentStatus { get; set; }
        public string ConsentStatusName => ConsentStatus.ToString();
        public DateTime? ParentSignedAt { get; set; }
        public string? ParentNotes { get; set; }
        public string? ParentSignature { get; set; }
        public DateTime? ConsentDeadline { get; set; }
    }
}
