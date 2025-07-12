using BusinessObjects;
using BusinessObjects.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.CheckUpRecordDTOs.Requests
{
    public class UpdateCheckupRecordRequestDTO
    {
        [Required]
        public Guid Id { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public VisionLevel? VisionLeft { get; set; }
        public VisionLevel? VisionRight { get; set; }
        public HearingLevel? Hearing { get; set; }
        public decimal? BloodPressureDiastolic { get; set; } //huyết áp 
        public Guid? ExaminedByNurseId { get; set; }              // Y tá thực hiện
        public string? Remarks { get; set; }              // Khuyến nghị
        public CheckupRecordStatus? Status { get; set; }           // Hoàn thành/Cần tái khám
    }

}
