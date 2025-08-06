using BusinessObjects.Common;
using DTOs.MedicationUsageRecord.Request;
using DTOs.MedicationUsageRecord.Respond;
using DTOs.ParentMedicationDeliveryDTOs.Respond;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IMedicationUsageRecordService
    {
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByDeliveryDetailIdAsync(Guid deliveryDetailId);
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByStudentIdAsync(Guid studentId);
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetByDateAsync(DateTime date);
        Task<ApiResult<MedicationUsageRecordResponseDTO>> UpdateTakenStatusAsync(UpdateMedicationUsageRecordDTO request);
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> BulkUpdateTakenStatusAsync(List<UpdateMedicationUsageRecordDTO> requests);
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> NurseBulkConfirmAsync(List<Guid> recordIds);
        Task<ApiResult<List<MedicationUsageRecordResponseDTO>>> GetPendingRecordsAsync();
        Task<ApiResult<List<ParentMedicationDeliveryResponseDTO>>> GetByDateParentAsync(DateTime date);
    }
}
