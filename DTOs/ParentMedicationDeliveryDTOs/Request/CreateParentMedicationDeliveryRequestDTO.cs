using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;

namespace DTOs.ParentMedicationDeliveryDTOs.Request
{
    public  class CreateParentMedicationDeliveryRequestDTO
    {
        [Required(ErrorMessage = "ID học sinh là bắt buộc")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "ID phụ huynh là bắt buộc")]
        public Guid ParentId { get; set; }

        //[Required(ErrorMessage = "ID người nhận là bắt buộc")]
        //public Guid ReceivedBy { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng thuốc giao phải lớn hơn 0")]
        public int QuantityDelivered { get; set; }
        public DateTime DeliveredAt { get; set; }
        public string? Notes { get; set; }
    }
}
