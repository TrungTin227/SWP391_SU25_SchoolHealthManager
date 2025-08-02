using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects;
using BusinessObjects.Common;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;

namespace Services.Helpers.Mapers
{
    public static class HealProfileMappings
    {   

        public static HealthProfile ToEntity(CreateHealProfileRequestDTO dto, Student student, Parent parent)
            {
                return new HealthProfile
                {
                    StudentId = student.Id,
                    ParentId = parent.UserId,
                    ProfileDate = DateTime.UtcNow,
                    Allergies = dto.Allergies ?? string.Empty,
                    ChronicConditions = dto.ChronicConditions ?? string.Empty,
                    TreatmentHistory = dto.TreatmentHistory ?? string.Empty,
                    Vision = dto.Vision ?? VisionLevel.Normal, // gán default nếu null
                    Hearing = dto.Hearing ?? HearingLevel.Normal, // gán default nếu null
                    VaccinationSummary = dto.VaccinationSummary ?? string.Empty,
                    Gender = dto.Gender
                };
            }

            public static HealProfileResponseDTO FromEntity(HealthProfile entity)
            {
                return new HealProfileResponseDTO
                {
                    ProfileId = entity.Id,
                    Version = entity.Version,
                    ProfileDate = entity.ProfileDate,
                    Allergies = entity.Allergies,
                    ChronicConditions = entity.ChronicConditions,
                    TreatmentHistory = entity.TreatmentHistory,
                    Vision = entity.Vision.ToString(),
                    Hearing = entity.Hearing.ToString(),
                    VaccinationSummary = entity.VaccinationSummary,
                    Gender = entity.Gender?.ToString()
                };
            }
    }
}
