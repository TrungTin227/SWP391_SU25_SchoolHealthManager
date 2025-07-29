using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyDTOs.Request
{
    public class ReconcileStockRequest
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng thực tế không được âm.")]
        public int ActualPhysicalCount { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; } // Lý do điều chỉnh, ví dụ: "Kiểm kê cuối năm"
    }
}
