using System.ComponentModel.DataAnnotations;

namespace DTOs.HealthEventDTOs.Request
{
    public class UpdateHealthEventRequest
    {
        [Required(ErrorMessage = "ID sự kiện y tế là bắt buộc")]
        public Guid HealthEventId { get; set; }

        /// <summary>
        /// Danh sách thuốc cần thêm vào sự kiện y tế
        /// </summary>
        //public List<CreateEventMedicationRequest>? EventMedications { get; set; }

        /// <summary>
        /// Danh sách vật tư y tế cần thêm vào sự kiện
        /// </summary>
        public List<CreateSupplyUsageRequest>? SupplyUsages { get; set; }
    }
}