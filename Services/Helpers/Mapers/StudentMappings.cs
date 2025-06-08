using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.StudentDTOs.Request;


namespace Services.Helpers.Mapers
{
    public static class StudentMappings
    {
        public static Student ToStudent(this AddStudentRequestDTO studentDto)
        {
            if (studentDto == null) return null;
            return new Student
            {
                StudentCode = studentDto.StudentCode,
                FirstName = studentDto.FirstName,
                LastName = studentDto.LastName,
                DateOfBirth = studentDto.DateOfBirth,
                Grade = studentDto.Grade,
                Section = studentDto.Section,
                Image = studentDto.Image,
                ParentUserId = studentDto.ParentID

            };
        }

    }
}
