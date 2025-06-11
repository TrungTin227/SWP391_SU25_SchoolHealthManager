using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicalSupplyDTOs.Request
{
    public class DeleteMedicalSuppliesRequest : BatchIdsRequest
    {
        [Display(Name = "Xóa vĩnh viễn")]
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreMedicalSuppliesRequest : BatchIdsRequest
    {
        // Kế thừa từ BatchIdsRequest, không cần thêm property nào
    }
}