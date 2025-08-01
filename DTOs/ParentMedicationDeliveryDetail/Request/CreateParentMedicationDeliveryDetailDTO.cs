using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentMedicationDeliveryDetail.Request
{
    public class CreateParentMedicationDeliveryDetailDTO
    {
        [Required(ErrorMessage = "Tên thuốc là bắt buộc")]
        public string MedicationName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Số lượng thuốc là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int QuantityDelivered { get; set; }
        
        public string? DosageInstruction { get; set; } // Hướng dẫn chung (tùy chọn)
        
        [Required(ErrorMessage = "Lịch uống thuốc là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 lần uống trong ngày")]
        public List<MedicationScheduleDTO> DailySchedule { get; set; } = new();
    }
    
    public class MedicationScheduleDTO
    {
        [Required(ErrorMessage = "Thời gian uống là bắt buộc")]
        public TimeSpan Time { get; set; } // VD: 08:00, 13:00, 20:00
        
        [Required(ErrorMessage = "Liều lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Liều lượng phải lớn hơn 0")]
        public int Dosage { get; set; } // Số viên cho lần uống này
        
        public string? Note { get; set; } // Ghi chú cho lần uống này (tùy chọn)
    }
}
