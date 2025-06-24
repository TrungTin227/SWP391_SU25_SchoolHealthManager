using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class StudentHelperController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;

        public StudentHelperController(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        /// <summary>
        /// API tổng hợp: Lấy thông tin khối/lớp và học sinh
        /// </summary>
        /// <param name="type">Loại dữ liệu: grades, sections, mapping, students</param>
        /// <param name="grades">Danh sách khối (chỉ dùng khi type=students)</param>
        /// <param name="sections">Danh sách lớp (chỉ dùng khi type=students)</param>
        [HttpGet]
        public async Task<IActionResult> GetData(
            [FromQuery] string type = "grades",
            [FromQuery] List<string>? grades = null,
            [FromQuery] List<string>? sections = null)
        {
            return type.ToLower() switch
            {
                "grades" => await GetGradesData(),
                "sections" => await GetSectionsData(),
                "mapping" => await GetMappingData(),
                "students" => await GetStudentsData(grades, sections),
                _ => BadRequest(new { Message = "Type không hợp lệ. Sử dụng: grades, sections, mapping, students" })
            };
        }

        private async Task<IActionResult> GetGradesData()
        {
            var grades = await _studentRepository.GetAvailableGradesAsync();
            return Ok(new { Data = grades, Message = "Lấy danh sách khối thành công" });
        }

        private async Task<IActionResult> GetSectionsData()
        {
            var sections = await _studentRepository.GetAvailableSectionsAsync();
            return Ok(new { Data = sections, Message = "Lấy danh sách lớp thành công" });
        }

        private async Task<IActionResult> GetMappingData()
        {
            var mapping = await _studentRepository.GetGradeSectionMappingAsync();
            return Ok(new { Data = mapping, Message = "Lấy mapping khối-lớp thành công" });
        }

        private async Task<IActionResult> GetStudentsData(List<string>? grades, List<string>? sections)
        {
            if (grades == null || sections == null)
            {
                return BadRequest(new { Message = "Grades và sections là bắt buộc khi type=students" });
            }

            var students = await _studentRepository.GetStudentsDTOByGradeAndSectionAsync(grades, sections);
            return Ok(new
            {
                Data = students,
                Count = students.Count,
                Message = "Lấy danh sách học sinh thành công"
            });
        }
    }
}