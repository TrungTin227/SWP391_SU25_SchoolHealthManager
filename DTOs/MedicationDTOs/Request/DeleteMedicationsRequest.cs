using DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace DTOs.MedicationDTOs.Request
{
    public class DeleteMedicationsRequest : BatchIdsRequest
    {
        [Display(Name = "Xóa vĩnh viễn")]
        public bool IsPermanent { get; set; } = false;
    }

    public class RestoreMedicationsRequest : BatchIdsRequest
    {
        // Kế thừa từ BatchIdsRequest, không cần thêm property nào
    }
}