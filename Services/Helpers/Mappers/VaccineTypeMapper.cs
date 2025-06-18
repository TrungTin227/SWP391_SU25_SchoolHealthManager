using Services.Mappers;

namespace Services.Helpers.Mappers
{
    public static class VaccineTypeMapper
    {
        public static VaccinationType MapFromCreateRequest(CreateVaccineTypeRequest request)
        {
            return new VaccinationType
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Group = request.Group,
                IsActive = request.IsActive,
                RecommendedAgeMonths = request.RecommendedAgeMonths,
                MinIntervalDays = request.MinIntervalDays
            };
        }

        public static void UpdateFromRequest(VaccinationType vaccineType, UpdateVaccineTypeRequest request)
        {
            if (!string.IsNullOrEmpty(request.Code))
                vaccineType.Code = request.Code;

            if (!string.IsNullOrEmpty(request.Name))
                vaccineType.Name = request.Name;

            if (!string.IsNullOrEmpty(request.Group))
                vaccineType.Group = request.Group;

            if (request.IsActive.HasValue)
                vaccineType.IsActive = request.IsActive.Value;

            if (request.RecommendedAgeMonths.HasValue)
                vaccineType.RecommendedAgeMonths = request.RecommendedAgeMonths.Value;

            if (request.MinIntervalDays.HasValue)
                vaccineType.MinIntervalDays = request.MinIntervalDays.Value;
        }

        public static VaccineTypeResponseDTO MapToResponseDTO(VaccinationType vaccineType)
        {
            return new VaccineTypeResponseDTO
            {
                Id = vaccineType.Id,
                Code = vaccineType.Code,
                Name = vaccineType.Name,
                Group = vaccineType.Group,
                IsActive = vaccineType.IsActive,
                RecommendedAgeMonths = vaccineType.RecommendedAgeMonths,
                MinIntervalDays = vaccineType.MinIntervalDays,
                CreatedAt = vaccineType.CreatedAt,
                UpdatedAt = vaccineType.UpdatedAt,
                IsDeleted = vaccineType.IsDeleted,
                TotalDoses = vaccineType.VaccineDoseInfos?.Count ?? 0,
                TotalSchedules = vaccineType.Schedules?.Count ?? 0,
                TotalMedicationLots = vaccineType.MedicationLots?.Count ?? 0
            };
        }

        public static VaccineTypeDetailResponseDTO MapToDetailResponseDTO(VaccinationType vaccineType)
        {
            var baseDto = MapToResponseDTO(vaccineType);

            return new VaccineTypeDetailResponseDTO
            {
                Id = baseDto.Id,
                Code = baseDto.Code,
                Name = baseDto.Name,
                Group = baseDto.Group,
                IsActive = baseDto.IsActive,
                RecommendedAgeMonths = baseDto.RecommendedAgeMonths,
                MinIntervalDays = baseDto.MinIntervalDays,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,
                TotalDoses = baseDto.TotalDoses,
                TotalSchedules = baseDto.TotalSchedules,
                TotalMedicationLots = baseDto.TotalMedicationLots,

                DoseInfos = vaccineType.VaccineDoseInfos?.Select(VaccineDoseInfoMapper.MapToResponseDTO).ToList()
                    ?? new List<VaccineDoseInfoResponseDTO>(),

                MedicationLots = vaccineType.MedicationLots?.Where(ml => !ml.IsDeleted)
                    .Select(MedicationLotMapper.MapToResponseDTO).ToList()
                    ?? new List<MedicationLotResponseDTO>()
            };
        }

        public static PagedList<VaccineTypeResponseDTO> ToPagedResult(
            PagedList<VaccinationType> source,
            IEnumerable<VaccineTypeResponseDTO> items)
        {
            var meta = source.MetaData;
            return new PagedList<VaccineTypeResponseDTO>(
                items.ToList(),
                meta.TotalCount,
                meta.CurrentPage,
                meta.PageSize);
        }
    }
}