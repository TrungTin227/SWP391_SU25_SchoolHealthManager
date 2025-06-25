namespace BusinessObjects.Common
{
    public enum Gender { Male, Female, Other }
    public enum Relationship { Father, Mother, Guardian, Other }
    public enum EventType { Accident, Fever, Fall, Disease, Other, VaccineReaction }
    public enum ScheduleStatus { Pending, InProgress, Completed, Cancelled }
    public enum ReferenceType { ConsentForm, HealthEvent, ScreeningResult }
    public enum RoleType{Student, Parent, SchoolNurse, Manager, Admin }
    public enum ScheduleType {Vaccination, Consultation }
    public enum StatusMedicationDelivery { Confirmed, Pending, Rejected }
    public enum EventCategory { General, Vaccination } // Phân loại sự kiện tiêm chủng 
    public enum EventStatus { Pending, InProgress, Resolved } // Trạng thái sự kiện
    public enum NotificationType{VaccinationSchedule, SupplyShortage }

    // Thêm enum cho quản lý thuốc
    public enum MedicationStatus { Active, Inactive, Discontinued }
    public enum MedicationCategory
    {
        Emergency,
        PainRelief,
        AntiAllergy,
        Antibiotic,
        TopicalTreatment,
        Disinfectant,
        SolutionAndVitamin,
        Digestive,
        ENT,
        Respiratory
    }
    public enum LotStatus { Available, LowStock, Expired, OutOfStock }
    public enum VisionLevel { Normal, Mild, Moderate, Severe }
    public enum HearingLevel { Normal, Mild, Moderate, Severe }
    public enum ReportType { HealthEvent, Vaccination, Checkup, Supply, Monthly, Annual }
    public enum NotificationStatus { Pending, Sent, Delivered, Failed }
    public enum FileType { PDF, Image, Document, Other }
    public enum Position { VaccinationNurse, HeadNurse, Other }

    public enum LotType{Medicine, Vaccine }
    public enum ParentConsentStatus
    {
        Pending = 0,        // Chờ phụ huynh ký
        Sent = 1,           // Đã gửi thông báo
        Approved = 2,       // Phụ huynh đã ký đồng ý
        Rejected = 3,       // Phụ huynh từ chối
        Expired = 4         // Hết hạn ký
    }
    public enum SessionStatus
    {
        Registered = 0,
        Present = 1,
        Absent = 2,
        Excused = 3,
        Completed = 4
    }
    public enum VaccinationCampaignStatus
    {
        Pending = 0,      // Vừa tạo, chờ xem xét
        InProgress = 1,   // Đã có thuốc/vật tư, đang thực hiện
        Resolved = 2,     // Đã hoàn thành
        Cancelled = 3     // Đã hủy
    }
    public enum VaccinationReactionSeverity
    {
        None = 0,          // Không có phản ứng
        Mild = 1,          // Nhẹ
        Moderate = 2,      // Vừa phải
        Severe = 3         // Nặng
    }
    public enum CheckupCampaignStatus
    {
        Planning = 0,           // Đang lên kế hoạch
        Scheduled = 1,          // Đã lên lịch
        InProgress = 2,         // Đang thực hiện
        Completed = 3,          // Hoàn thành
        Cancelled = 4           // Đã hủy
    }

    public enum CheckupRecordStatus
    {
        Pending = 0,            // Chờ khám
        Completed = 1,          // Đã khám xong
        RequiresFollowUp = 2,   // Cần tái khám
        Referred = 3            // Chuyển tuyến
    }
    public enum CheckupScheduleStatus
    {
        Pending = 0,        // Chờ phụ huynh đồng ý
        Approved = 1,       // Phụ huynh đã đồng ý
        Declined = 2,       // Phụ huynh từ chối
        Completed = 3,      // Đã khám xong
        NoShow = 4          // Không đến khám
    }
}
