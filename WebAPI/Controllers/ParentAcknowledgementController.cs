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
            var secret = _config["Jwt:Secret"];
            var payload = $"{id}|{DateTime.UtcNow:yyyyMMdd}";
            var computed = Convert.ToBase64String(
                System.Security.Cryptography.HMACSHA256
                    .Create(secret)
                    .ComputeHash(Encoding.UTF8.GetBytes(payload)));

            if (token != computed)
                return BadRequest("Liên kết không hợp lệ hoặc đã hết hạn.");

            var result = await _service.RecordParentAckAsync(id);

            if (result.IsSuccess)
                return Redirect("https://localhost:5173/parent/ack-success");

            return BadRequest(result.Message ?? "Lỗi khi xác nhận");
        }
    }
}
