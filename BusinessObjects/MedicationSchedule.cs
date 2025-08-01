using System;

namespace BusinessObjects
{
    public class MedicationSchedule : BaseEntity
    {
        public Guid Id { get; set; }

        // Quan hệ với bảng chi tiết thuốc
        public Guid ParentMedicationDeliveryDetailId { get; set; }
        public ParentMedicationDeliveryDetail ParentMedicationDeliveryDetail { get; set; } = default!;

        // Thông tin lịch uống
        public TimeSpan Time { get; set; } // VD: 08:00, 13:00, 20:00
        public int Dosage { get; set; }    // Số viên cho lần uống này
        public string? Note { get; set; }  // Ghi chú cho lần uống này

        // Quan hệ với bảng record sử dụng
        public List<MedicationUsageRecord> UsageRecords { get; set; } = new();
    }
} 