using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NurseProfileController : Controller
    {
        private readonly INurseProfileService _nurseProfileService;

        public NurseProfileController(INurseProfileService nurseProfileService)
        {
            _nurseProfileService = nurseProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNurseAsync()
        {
            var result = await _nurseProfileService.GetAllNurseAsync();
            if (result.IsSuccess)
                return Ok(result);
            else
                return BadRequest(result);
        }
    }
}
