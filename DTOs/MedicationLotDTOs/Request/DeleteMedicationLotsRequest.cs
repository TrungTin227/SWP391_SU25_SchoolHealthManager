using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicationLotDTOs.Request
{
    public class DeleteMedicationLotsRequest : BatchIdsRequest
    {
        [Display(Name = "Xóa vĩnh viễn")]
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreMedicationLotsRequest : BatchIdsRequest
    {
    }
}