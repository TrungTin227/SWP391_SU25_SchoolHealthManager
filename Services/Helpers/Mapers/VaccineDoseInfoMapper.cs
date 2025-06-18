using DTOs.VaccineDoseInfoDTOs.Request;
using DTOs.VaccineDoseInfoDTOs.Response;

namespace Services.Helpers.Mappers
{
    public static class VaccineDoseInfoMapper
    {
        public static VaccineDoseInfo MapFromCreateRequest(CreateVaccineDoseInfoRequest request)
        {
            return new VaccineDoseInfo
            {
                Id = Guid.NewGuid(),
                VaccineTypeId = request.VaccineTypeId,
                DoseNumber = request.DoseNumber,
                RecommendedAgeMonths = request.RecommendedAgeMonths,
                MinIntervalDays = request.MinIntervalDays,
                PreviousDoseId = request.PreviousDoseId
            };
        }

        public static void UpdateFromRequest(VaccineDoseInfo doseInfo, UpdateVaccineDoseInfoRequest request)
        {
            if (request.DoseNumber.HasValue)
                doseInfo.DoseNumber = request.DoseNumber.Value;

            if (request.RecommendedAgeMonths.HasValue)
                doseInfo.RecommendedAgeMonths = request.RecommendedAgeMonths.Value;

            if (request.MinIntervalDays.HasValue)
                doseInfo.MinIntervalDays = request.MinIntervalDays.Value;

            if (request.PreviousDoseId.HasValue)
                doseInfo.PreviousDoseId = request.PreviousDoseId.Value;
        }

        public static VaccineDoseInfoResponseDTO MapToResponseDTO(VaccineDoseInfo doseInfo)
        {
            return new VaccineDoseInfoResponseDTO
            {
                Id = doseInfo.Id,
                VaccineTypeId = doseInfo.VaccineTypeId,
                VaccineTypeName = doseInfo.VaccineType?.Name ?? string.Empty,
                VaccineTypeCode = doseInfo.VaccineType?.Code ?? string.Empty,
                DoseNumber = doseInfo.DoseNumber,
                RecommendedAgeMonths = doseInfo.RecommendedAgeMonths,
                MinIntervalDays = doseInfo.MinIntervalDays,
                PreviousDoseId = doseInfo.PreviousDoseId,
                PreviousDoseName = doseInfo.PreviousDose != null
                    ? $"Mũi {doseInfo.PreviousDose.DoseNumber}"
                    : null,
                TotalNextDoses = doseInfo.NextDoses?.Count ?? 0,
                CreatedAt = doseInfo.CreatedAt,
                UpdatedAt = doseInfo.UpdatedAt,
                IsDeleted = doseInfo.IsDeleted
            };
        }

        public static VaccineDoseInfoDetailResponseDTO MapToDetailResponseDTO(VaccineDoseInfo doseInfo)
        {
            var baseDto = MapToResponseDTO(doseInfo);

            return new VaccineDoseInfoDetailResponseDTO
            {
                Id = baseDto.Id,
                VaccineTypeId = baseDto.VaccineTypeId,
                VaccineTypeName = baseDto.VaccineTypeName,
                VaccineTypeCode = baseDto.VaccineTypeCode,
                DoseNumber = baseDto.DoseNumber,
                RecommendedAgeMonths = baseDto.RecommendedAgeMonths,
                MinIntervalDays = baseDto.MinIntervalDays,
                PreviousDoseId = baseDto.PreviousDoseId,
                PreviousDoseName = baseDto.PreviousDoseName,
                TotalNextDoses = baseDto.TotalNextDoses,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,

                NextDoses = doseInfo.NextDoses?.Select(MapToResponseDTO).ToList()
                    ?? new List<VaccineDoseInfoResponseDTO>(),

                PreviousDose = doseInfo.PreviousDose != null
                    ? MapToResponseDTO(doseInfo.PreviousDose)
                    : null
            };
        }

        public static PagedList<VaccineDoseInfoResponseDTO> ToPagedResult(
            PagedList<VaccineDoseInfo> source,
            IEnumerable<VaccineDoseInfoResponseDTO> items)
        {
            var meta = source.MetaData;
            return new PagedList<VaccineDoseInfoResponseDTO>(
                items.ToList(),
                meta.TotalCount,
                meta.CurrentPage,
                meta.PageSize);
        }
    }
}