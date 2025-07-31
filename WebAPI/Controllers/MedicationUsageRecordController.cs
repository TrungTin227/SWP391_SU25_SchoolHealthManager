using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using DTOs.MedicationUsageRecord.Request;
using DTOs.MedicationUsageRecord.Respond;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/medication-usage-records")]
    [Authorize]
    public class MedicationUsageRecordController : ControllerBase
    {
        private readonly IMedicationUsageRecordService _medicationUsageRecordService;

        public MedicationUsageRecordController(IMedicationUsageRecordService medicationUsageRecordService)
        {
            _medicationUsageRecordService = medicationUsageRecordService;
        }

        /// <summary>
        /// Lấy danh sách record uống thuốc theo delivery detail
        /// </summary>
        [HttpGet("delivery-detail/{deliveryDetailId}")]
        public async Task<IActionResult> GetByDeliveryDetailId(Guid deliveryDetailId)
        {
            var result = await _medicationUsageRecordService.GetByDeliveryDetailIdAsync(deliveryDetailId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách record uống thuốc theo học sinh
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var result = await _medicationUsageRecordService.GetByStudentIdAsync(studentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách record uống thuốc theo ngày
        /// </summary>
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetByDate(DateTime date)
        {
            var result = await _medicationUsageRecordService.GetByDateAsync(date);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật trạng thái uống thuốc
        /// </summary>
        [HttpPatch("update-taken")]
        public async Task<IActionResult> UpdateTakenStatus([FromBody] UpdateMedicationUsageRecordDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _medicationUsageRecordService.UpdateTakenStatusAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Cập nhật hàng loạt trạng thái uống thuốc
        /// </summary>
        [HttpPatch("bulk-update")]
        public async Task<IActionResult> BulkUpdateTakenStatus([FromBody] List<UpdateMedicationUsageRecordDTO> requests)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _medicationUsageRecordService.BulkUpdateTakenStatusAsync(requests);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // --------- Nurse-friendly APIs ---------

        /// <summary>
        /// Lấy danh sách record uống thuốc hôm nay cho y tá (nurse)
        /// </summary>
        [HttpGet("today")]
        //[Authorize(Roles = "Nurse")]
        public async Task<IActionResult> GetTodayRecords()
        {
            var today = DateTime.UtcNow.Date;
            var result = await _medicationUsageRecordService.GetByDateAsync(today);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách record uống thuốc chưa xác nhận (IsTaken = false) cho y tá
        /// </summary>
        [HttpGet("pending")]
        //[Authorize(Roles = "Nurse")]
        public async Task<IActionResult> GetPendingRecords()
        {
            var result = await _medicationUsageRecordService.GetPendingRecordsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Xác nhận đã uống thuốc cho nhiều record (nurse bulk confirm)
        /// </summary>
        [HttpPatch("nurse/bulk-confirm")]
        //[Authorize(Roles = "Nurse")]
        public async Task<IActionResult> NurseBulkConfirm([FromBody] List<Guid> recordIds)
        {
            if (recordIds == null || !recordIds.Any())
                return BadRequest("Danh sách recordId không được để trống.");

            var result = await _medicationUsageRecordService.NurseBulkConfirmAsync(recordIds);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}