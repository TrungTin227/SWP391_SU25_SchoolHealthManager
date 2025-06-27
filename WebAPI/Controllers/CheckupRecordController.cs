using DTOs.CheckUpRecordDTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckupRecordController : Controller
    {
        private readonly ICheckupRecordService _checkupRecordService;
        public CheckupRecordController(ICheckupRecordService checkupRecordService)
        {
            _checkupRecordService = checkupRecordService;
        }
        [HttpPost("create-checkup-record")]
        public async Task<IActionResult> CreateCheckupRecord([FromBody] CreateCheckupRecordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request == null)
            {
                return BadRequest(new { Message = "Yêu cầu nhập dữ liệu!!" });
            }
            var result = await _checkupRecordService.CreateCheckupRecordAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
