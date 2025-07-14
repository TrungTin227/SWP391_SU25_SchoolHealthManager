public class CreateVaccinationRecordResponse
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    // Thông tin student
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string StudentCode { get; set; }
    // Thông tin vắc xin
    public string VaccineName { get; set; }
    public string LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    // Thông tin người tiêm
    public Guid VaccinatedById { get; set; } // <-- Thêm trường này
    public string VaccinatedBy { get; set; }
    // Ngày tiêm
    public DateTime AdministeredDate { get; set; }
    // Phản ứng sau tiêm
    public string? ReactionFollowup24h { get; set; }
    public string? ReactionFollowup72h { get; set; }
    // Ghi chú & trạng thái
    public string? Notes { get; set; }
    public string Status { get; set; }
}
