using DTOs.ParentVaccinationDTOs.Request;
using DTOs.ParentVaccinationDTOs.Response;
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