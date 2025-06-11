using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyLotDTOs.Request
{
    public class DeleteMedicalSupplyLotsRequest : BatchIdsRequest
    {
        [Display(Name = "Xóa vĩnh viễn")]
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreMedicalSupplyLotsRequest : BatchIdsRequest
    {
    }
}