using BusinessObjects;
using BusinessObjects.Common;

public class ParentMedicationDelivery : BaseEntity
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = default!;

    public Guid ParentId { get; set; }
    public Parent Parent { get; set; } = default!;

    public Guid? ReceivedBy { get; set; }
    public User? ReceivedUser { get; set; } = default!;

    public DateTime DeliveredAt { get; set; }
    public string? Notes { get; set; }
    public StatusMedicationDelivery Status { get; set; }

    // Quan hệ
    public List<ParentMedicationDeliveryDetail> Details { get; set; } = new();
    public List<FileAttachment> Attachments { get; set; } = new();
}
