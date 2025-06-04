using BusinessObjects.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class ParentMedicationDelivery : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; }

        // SỬA: Đổi từ DeliveredBy thành ParentId để rõ ràng
        public Guid ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public Parent Parent { get; set; }

        public Guid ReceivedBy { get; set; }
        [ForeignKey(nameof(ReceivedBy))]
        public User ReceivedUser { get; set; }

        public int QuantityDelivered { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string Notes { get; set; }
        public StatusMedicationDelivery Status { get; set; }
    }
}