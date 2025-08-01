namespace Services.Commons.Gmail
{
    public interface ISchoolHealthEmailService
    {
        // Parent notifications
        Task SendVaccinationConsentRequestAsync(string parentEmail, string studentName, string vaccineName, DateTime scheduledDate);
        Task SendHealthCheckupNotificationAsync(string parentEmail, string studentName, DateTime checkupDate);
        Task SendMedicationDeliveryConfirmationAsync(string parentEmail, string studentName, string medicationName);
        Task SendHealthEventNotificationAsync(string parentEmail, string studentName, string eventDescription, string treatmentProvided);
        Task SendMedicationDeliverdAsync(string parentEmail, string studentName, string medicationName);

        // Nurse notifications
        Task SendDailyHealthReportAsync(List<string> nurseEmails, string reportContent);
        Task SendMedicalSupplyLowStockAlertAsync(List<string> adminEmails, List<string> lowStockItems);
        Task SendEmergencyHealthEventAlertAsync(List<string> staffEmails, string studentName, string emergencyDetails);

        // System notifications
        Task SendVaccinationCampaignReminderAsync(List<string> staffEmails, string campaignName, DateTime startDate);
        Task SendHealthCheckupCampaignReminderAsync(List<string> staffEmails, string campaignName, DateTime startDate);
        Task SendMonthlyHealthReportAsync(List<string> adminEmails, string reportPeriod, string reportSummary);
        Task SendHealthEventAckMailAsync(
            string parentEmail,
            string studentName,
            string eventDescription,
            string treatmentProvided,
            DateTime occurredAt, 
            string location,     
            string symptoms,     
            Guid eventId,
            string ackToken);
        Task SendHospitalReferralAckAsync(
    string parentEmail,
    string studentName,
    string referralHospital,
    DateTime departureTime,
    string transportBy,
    string initialSymptoms,
    string injuredBodyParts,
    string firstAidDescription,
    Guid eventId,
    string ackToken);
    }
}