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
}
