using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DTOs.HealProfile.Requests;
using DTOs.HealProfile.Responds;

namespace Services.Helpers.Mapers
{
    public static class HealProfileMappings
    {
            public static HealthProfile ToEntity(CreateHealProfileRequestDTO dto)
            {
                return new HealthProfile
                {
                    StudentId = dto.StudentId,
                    ParentId = dto.ParentId,
                    ProfileDate = DateTime.UtcNow,
                    Allergies = dto.Allergies ?? string.Empty,
                    ChronicConditions = dto.ChronicConditions ?? string.Empty,
                    TreatmentHistory = dto.TreatmentHistory ?? string.Empty,
                    Vision = dto.Vision ?? VisionLevel.Normal, // gán default nếu null
                    Hearing = dto.Hearing ?? HearingLevel.Normal, // gán default nếu null
                    VaccinationSummary = dto.VaccinationSummary ?? string.Empty
                };
            }

            public static HealProfileResponseDTO FromEntity(HealthProfile entity)
            {
                return new HealProfileResponseDTO
                {
                    Version = entity.Version,
                    ProfileDate = entity.ProfileDate,
                    Allergies = entity.Allergies,
                    ChronicConditions = entity.ChronicConditions,
                    TreatmentHistory = entity.TreatmentHistory,
                    Vision = entity.Vision.ToString(),
                    Hearing = entity.Hearing.ToString(),
                    VaccinationSummary = entity.VaccinationSummary
                };
            }
    }
}
