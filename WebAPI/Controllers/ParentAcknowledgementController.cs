using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/health-events")]
    public class ParentAcknowledgementController : ControllerBase
    {
        private readonly IHealthEventService _service;
        private readonly IConfiguration _config;

        public ParentAcknowledgementController(IHealthEventService service, IConfiguration config)
        {
            _service = service;
            _config = config;
        }

        [HttpPut("{id:guid}/parent-ack")]
        [AllowAnonymous]
        public async Task<IActionResult> ParentAcknowledge(Guid id, [FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return BadRequest(new { success = false, message = "Token không hợp lệ." });

                var correctedToken = token.Replace(' ', '+');
                var secret = _config["JwtSettings:Key"];

                if (string.IsNullOrWhiteSpace(secret))
                    return StatusCode(500, new { success = false, message = "Cấu hình hệ thống không hợp lệ." });

                var isValidToken = false;
                var maxDaysValid = 7;

                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var currentVietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

                for (int i = 0; i < maxDaysValid; i++)
                {
                    var checkDate = currentVietnamTime.AddDays(-i);
                    var payload = $"{id}|{checkDate:yyyyMMdd}";
                    using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
                    var computed = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

                    if (correctedToken == computed)
                    {
                        isValidToken = true;
                        break;
                    }
                }

                if (!isValidToken)
                    return BadRequest(new { success = false, message = "Liên kết không hợp lệ hoặc đã hết hạn." });

                var result = await _service.RecordParentAckAsync(id);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Xác nhận thành công",
                        redirectUrl = "/parent/ack-success"
                    });
                }

                return BadRequest(new { success = false, message = result.Message ?? "Lỗi khi xác nhận" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại sau." });
            }
        }
    }
}