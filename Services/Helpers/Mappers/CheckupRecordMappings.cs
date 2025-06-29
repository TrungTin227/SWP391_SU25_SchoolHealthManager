using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers.Mappers
{
    public static class CheckupRecordMappings
    {
        public static CheckupRecord MapToEntity(CreateCheckupRecordRequestDTO dto, Student student)
        {
            var checkupRecord = new CheckupRecord
            {
                Id = Guid.NewGuid(), // Tạo mới ID
                ScheduleId = dto.ScheduleId,
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg,
                VisionLeft = dto.VisionLeft,
                VisionRight = dto.VisionRight,
                Hearing = dto.Hearing,
                BloodPressureDiastolic = dto.BloodPressureDiastolic,
                ExaminedByNurseId = dto.ExaminedByNurseId,
                ExaminedAt = dto.ExaminedAt,
                Remarks = dto.Remarks,
                Status = dto.Status,
                CounselingAppointments = new List<CounselingAppointment>()
            };

            // Nếu có danh sách khám lại thì gán vào
            if (dto.CounselingAppointment != null && dto.CounselingAppointment.Any() && dto.Status == CheckupRecordStatus.RequiresFollowUp)
            {
                foreach (var counselingDto in dto.CounselingAppointment)
                {
                    checkupRecord.CounselingAppointments.Add(new CounselingAppointment
                    {
                        Id = Guid.NewGuid(),
                        AppointmentDate = counselingDto.AppointmentDate,
                        Duration = counselingDto.Duration,
                        StudentId = student.Id, // FK
                        ParentId = student.ParentUserId, // FK
                        Purpose = counselingDto.Purpose, // Gán đầy đủ trường Purpose
                        StaffUserId = counselingDto.StaffUserId,
                        CheckupRecordId = checkupRecord.Id, // FK
                        Status = ScheduleStatus.Pending,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = counselingDto.StaffUserId // Thay thế với user Id thực tế
                    });
                }
            }

            return checkupRecord;
        }
        public static CheckupRecordRespondDTO MapToRespondDTO(CheckupRecord entity)
        {
            return new CheckupRecordRespondDTO
            {
                Id = entity.Id,
                ScheduleId = entity.ScheduleId,
                HeightCm = entity.HeightCm,
                WeightKg = entity.WeightKg,
                VisionLeft = entity.VisionLeft,
                VisionRight = entity.VisionRight,
                Hearing = entity.Hearing,
                BloodPressureDiastolic = entity.BloodPressureDiastolic,
                ExaminedByNurseId = entity.ExaminedByNurseId,
                ExaminedAt = entity.ExaminedAt,
                Remarks = entity.Remarks,
                Status = entity.Status,

                CounselingAppointments = entity.CounselingAppointments?
                    .Select(c => c.Id)
                    .ToList()
            };

        }
    }
}
