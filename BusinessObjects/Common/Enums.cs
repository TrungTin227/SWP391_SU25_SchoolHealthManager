namespace BusinessObjects.Common
{
    public enum Gender { Male, Female, Other }
    public enum Relationship { Father, Mother, Guardian, Other }
    public enum EventType { Accident, Fever, Fall, Disease, Other }
    public enum ScheduleStatus { Scheduled, Completed, Cancelled }
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
    public enum CheckupScheduleStatus { NotConfirmed, Confirmed }
    public enum VisionLevel { Normal, Mild, Moderate, Severe }
    public enum HearingLevel { Normal, Mild, Moderate, Severe }
    public enum ReportType { HealthEvent, Vaccination, Checkup, Supply, Monthly, Annual }
    public enum NotificationStatus { Pending, Sent, Delivered, Failed }
    public enum FileType { PDF, Image, Document, Other }
}
