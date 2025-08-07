using BusinessObjects;
using BusinessObjects.Common;
using DTOs.ParentMedicationDeliveryDetail.Request;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ParentMedicationDeliveryDTOs.Request
{
    public  class CreateParentMedicationDeliveryRequestDTO
    {
        [Required(ErrorMessage = "ID học sinh là bắt buộc")]
        public Guid StudentId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<CreateParentMedicationDeliveryDetailDTO> Medications { get; set; } = new();
        public List<IFormFile>? PrescriptionImages { get; set; } = new();
    }
}
