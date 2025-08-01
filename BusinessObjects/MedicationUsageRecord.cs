using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class MedicationUsageRecord : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid DeliveryDetailId { get; set; }
        public ParentMedicationDeliveryDetail DeliveryDetail { get; set; } = default!;

        // Quan hệ với lịch uống thuốc
        public Guid MedicationScheduleId { get; set; }
        public MedicationSchedule MedicationSchedule { get; set; } = default!;

        public DateTime ScheduledAt { get; set; }
        public DateTime? TakenAt { get; set; }
        public bool IsTaken { get; set; }
        public string? Note { get; set; } // Ví dụ: học sinh từ chối uống, vắng mặt, ...

        public Guid? CheckedBy { get; set; }
        public User? Nurse { get; set; }
    }

}
