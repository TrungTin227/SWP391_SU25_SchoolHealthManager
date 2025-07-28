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
    // Thông tin người tiêm
    public Guid VaccinatedById { get; set; } // <-- Thêm trường này
    public string VaccinatedBy { get; set; }
    // Ngày tiêm
    public DateTime AdministeredDate { get; set; }
    // Phản ứng sau tiêm
    public string? ReactionFollowup24h { get; set; }
    public string? ReactionFollowup72h { get; set; }
    public int ReactionSeverity { get; set; }
    //trạng thái phiếu tiêm
    public string SessionStatus { get; set; }
}
