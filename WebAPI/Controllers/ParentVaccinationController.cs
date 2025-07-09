using DTOs.ParentVaccinationDTOs.Request;
using DTOs.ParentVaccinationDTOs.Response;
using DTOs.VaccinationRecordDTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/parent/vaccinations")]
    //[Authorize(Roles = "Parent")]
    public class ParentVaccinationController : ControllerBase
    {
        private readonly IParentVaccinationService _parentVaccinationService;
        private readonly ILogger<ParentVaccinationController> _logger;

        public ParentVaccinationController(
            IParentVaccinationService parentVaccinationService,
            ILogger<ParentVaccinationController> logger)
        {
            _parentVaccinationService = parentVaccinationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateParentVaccinationRecord([FromBody] List<CreateParentVaccinationRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest("Danh sách tiêm chủng không được để trống.");

            var result = await _parentVaccinationService.CreateParentVaccinationListAsync(requests);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 2. Cập nhật danh sách tiêm chủng
        [HttpPut]
        public async Task<IActionResult> UpdateParentVaccinationRecord([FromBody] List<UpdateParentVaccinationRequestDTO> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest("Danh sách cần cập nhật không được để trống.");

            var result = await _parentVaccinationService.UpdateParentVaccinationListAsync(requests);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 3. Xoá mềm danh sách tiêm chủng
        [HttpDelete]
        public async Task<IActionResult> SoftDeleteParentVaccinationRecord([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách ID cần xoá không được để trống.");

            var result = await _parentVaccinationService.SoftDeleteParentVaccinationRangeAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // 4. Khôi phục danh sách tiêm chủng
        [HttpPut("restore")]
        public async Task<IActionResult> RestoreParentVaccinationRecord([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Danh sách ID cần khôi phục không được để trống.");

            var result = await _parentVaccinationService.RestoreParentVaccinationRangeAsync(ids);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("student-id/{studentId}")]
        public async Task<IActionResult> GetByStudentId(Guid studentId)
        {
            var result = await _parentVaccinationService.GetByStudentIdAsync(studentId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("student-code/{studentCode}")]
        public async Task<IActionResult> GetByStudentCode(string studentCode)
        {
            var result = await _parentVaccinationService.GetByStudentCodeAsync(studentCode);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("parent-user/{userId}")]
        public async Task<IActionResult> GetByParentUserId(Guid userId)
        {
            var result = await _parentVaccinationService.GetByParentUserIdAsync(userId);
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        #region Lấy danh sách lịch tiêm

        /// <summary>
        /// Lấy danh sách lịch tiêm chờ ký đồng ý
        /// </summary>
        [HttpGet("pending-consent")]
        public async Task<IActionResult> GetPendingConsentSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _parentVaccinationService
                .GetVaccinationSchedulesByStatusAsync(ParentActionStatus.PendingConsent, pageNumber, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lịch tiêm sắp diễn ra (đã đồng ý)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _parentVaccinationService
                .GetVaccinationSchedulesByStatusAsync(ParentActionStatus.Approved, pageNumber, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lịch tiêm đã hoàn thành
        /// </summary>
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _parentVaccinationService
                .GetVaccinationSchedulesByStatusAsync(ParentActionStatus.Completed, pageNumber, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy danh sách lịch tiêm cần theo dõi
        /// </summary>
        [HttpGet("follow-up")]
        public async Task<IActionResult> GetFollowUpSchedules(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _parentVaccinationService
                .GetVaccinationSchedulesByStatusAsync(ParentActionStatus.RequiresFollowUp, pageNumber, pageSize);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy chi tiết lịch tiêm
        /// </summary>
        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetScheduleDetail(Guid scheduleId)
        {
            var result = await _parentVaccinationService.GetVaccinationScheduleDetailAsync(scheduleId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Xử lý đồng ý

        /// <summary>
        /// Ký đồng ý/từ chối tiêm chủng
        /// </summary>
        [HttpPost("consent")]
        public async Task<IActionResult> SubmitConsent([FromBody] ParentConsentRequestDTO request)
        {
            var result = await _parentVaccinationService.SubmitConsentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Ký đồng ý/từ chối hàng loạt
        /// </summary>
        [HttpPost("consent/batch")]
        public async Task<IActionResult> SubmitBatchConsent([FromBody] BatchParentConsentRequestDTO request)
        {
            var result = await _parentVaccinationService.SubmitBatchConsentAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Lịch sử tiêm chủng

        /// <summary>
        /// Lấy lịch sử tiêm chủng của tất cả con
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetVaccinationHistory()
        {
            var result = await _parentVaccinationService.GetVaccinationHistoryAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy lịch sử tiêm chủng của một học sinh
        /// </summary>
        [HttpGet("history/student/{studentId}")]
        public async Task<IActionResult> GetStudentVaccinationHistory(Guid studentId)
        {
            var result = await _parentVaccinationService.GetStudentVaccinationHistoryAsync(studentId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion

        #region Báo cáo phản ứng

        /// <summary>
        /// Báo cáo phản ứng sau tiêm
        /// </summary>
        //[HttpPost("reaction-report")]
        //public async Task<IActionResult> ReportVaccinationReaction([FromBody] ReportVaccinationReactionRequestDTO request)
        //{
        //    var result = await _parentVaccinationService.ReportVaccinationReactionAsync(request);
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        #endregion

        #region Thông báo và thống kê

        /// <summary>
        /// Lấy thông báo chờ xử lý
        /// </summary>
        [HttpGet("notifications/pending")]
        public async Task<IActionResult> GetPendingNotifications()
        {
            var result = await _parentVaccinationService.GetPendingNotificationsAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Lấy thống kê tổng quan
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetVaccinationSummary()
        {
            var result = await _parentVaccinationService.GetVaccinationSummaryAsync();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        #endregion
    }
}