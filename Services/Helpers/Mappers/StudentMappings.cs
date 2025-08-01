﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;
using DTOs.StudentDTOs.Request;
using DTOs.StudentDTOs.Response;
using static Quartz.Logging.OperationName;


namespace Services.Helpers.Mapers
{
    public static class StudentMappings
    {
        public static Student AddStudentToStudent(this AddStudentRequestDTO studentDto)
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
                Gender = studentDto.Gender,
                ParentUserId = studentDto.ParentID

            };
        }

        public static Student ToUpdatedStudent(this UpdateStudentRequestDTO dto, Student existingStudent)
        {
            if (dto == null || existingStudent == null) return existingStudent;

            // Chỉ gán nếu DTO có giá trị
            if (!string.IsNullOrEmpty(dto.StudentCode))
                existingStudent.StudentCode = dto.StudentCode;

            if (!string.IsNullOrEmpty(dto.FirstName))
                existingStudent.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                existingStudent.LastName = dto.LastName;

            if (dto.DateOfBirth.HasValue)
                existingStudent.DateOfBirth = dto.DateOfBirth.Value;

            if (!string.IsNullOrEmpty(dto.Grade))
                existingStudent.Grade = dto.Grade;

            if (!string.IsNullOrEmpty(dto.Section))
                existingStudent.Section = dto.Section;

            if (!string.IsNullOrEmpty(dto.Image))
                existingStudent.Image = dto.Image;

            if (dto.Gender.HasValue)
                existingStudent.Gender = dto.Gender.Value;

            return existingStudent;
        }

        public static GetAllStudentDTO ToGetAllStudent(this Student dto)
        {
            if (dto == null) return null;
            return new GetAllStudentDTO
            {
                Id = dto.Id,
                StudentCode = dto.StudentCode,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Grade = dto.Grade,
                Section = dto.Section,
                Image = dto.Image,
                Gender = dto.Gender
            };
        }

    }
}
