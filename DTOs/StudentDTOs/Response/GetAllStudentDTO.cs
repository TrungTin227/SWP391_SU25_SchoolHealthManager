using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.StudentDTOs.Response
{
    public class GetAllStudentDTO
    {
        public Guid Id { get; set; }
        public string StudentCode { get; set; }      // Mã định danh học sinh (ví dụ: HS2025001)
        public string FirstName { get; set; }        // Tên
        public string LastName { get; set; }         // Họ và tên đệm
        public DateTime DateOfBirth { get; set; }    // Ngày sinh
        public string? Grade { get; set; }            // Khối lớp (ví dụ: "5")
        public string? Section { get; set; }          // Lớp (ví dụ: "5A")
        public string? Image { get; set; }            // Ảnh đại diện (URL hoặc path)
    }
}
