namespace Services.Helpers.Mappers
{
    public static class VaccinationCampaignMapper
    {
        public static VaccinationCampaign MapFromCreateRequest(CreateVaccinationCampaignRequest request)
        {
            return new VaccinationCampaign
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                SchoolYear = request.SchoolYear,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = VaccinationCampaignStatus.Pending
            };
        }

        public static void UpdateFromRequest(VaccinationCampaign campaign, UpdateVaccinationCampaignRequest request)
        {
            if (!string.IsNullOrEmpty(request.Name))
                campaign.Name = request.Name;

            if (request.Description != null)
                campaign.Description = request.Description;

            if (request.StartDate.HasValue)
                campaign.StartDate = request.StartDate.Value;

            if (request.EndDate.HasValue)
                campaign.EndDate = request.EndDate.Value;
        }

        public static VaccinationCampaignResponseDTO MapToResponseDTO(VaccinationCampaign campaign)
        {
            return new VaccinationCampaignResponseDTO
            {
                Id = campaign.Id,
                Name = campaign.Name,
                SchoolYear = campaign.SchoolYear ?? string.Empty,
                Description = campaign.Description,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                TotalSchedules = campaign.Schedules?.Count ?? 0,
                CompletedSchedules = campaign.Schedules?.Count(s => s.ScheduleStatus == ScheduleStatus.Completed) ?? 0,
                IsActive = campaign.Status == VaccinationCampaignStatus.InProgress,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                IsDeleted = campaign.IsDeleted,
                CreatedBy = campaign.CreatedBy.ToString(),
                UpdatedBy = campaign.UpdatedBy.ToString()
            };
        }

        public static VaccinationCampaignDetailResponseDTO MapToDetailResponseDTO(VaccinationCampaign campaign)
        {
            var baseDto = MapToResponseDTO(campaign);

            return new VaccinationCampaignDetailResponseDTO
            {
                Id = baseDto.Id,
                Name = baseDto.Name,
                SchoolYear = baseDto.SchoolYear,
                Description = baseDto.Description,
                StartDate = baseDto.StartDate,
                EndDate = baseDto.EndDate,
                Status = baseDto.Status,
                TotalSchedules = baseDto.TotalSchedules,
                CompletedSchedules = baseDto.CompletedSchedules,
                IsActive = baseDto.IsActive,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,
                CreatedBy = baseDto.CreatedBy,
                UpdatedBy = baseDto.UpdatedBy,
 
                Schedules = campaign.Schedules?
                    .Select(MapToScheduleResponseDTO)
                    .ToList() ?? new List<VaccinationScheduleResponseDTO>()
            };
        }

        public static VaccinationScheduleResponseDTO MapToScheduleResponseDTO(VaccinationSchedule schedule)
        {
            return new VaccinationScheduleResponseDTO
            {
                Id = schedule.Id,
                VaccinationTypeName = schedule.VaccinationType?.Name ?? string.Empty,
                ScheduledAt = schedule.ScheduledAt,
                ScheduleStatus = schedule.ScheduleStatus,
                TotalStudents = schedule.SessionStudents?.Count ?? 0,
                CompletedRecords = schedule.SessionStudents?.SelectMany(ss => ss.VaccinationRecords).Count() ?? 0 
            };
        }

        public static PagedList<VaccinationCampaignResponseDTO> ToPagedResult(
            PagedList<VaccinationCampaign> source,
            IEnumerable<VaccinationCampaignResponseDTO> items)
        {
            var meta = source.MetaData;
            return new PagedList<VaccinationCampaignResponseDTO>(
                items.ToList(),
                meta.TotalCount,
                meta.CurrentPage,
                meta.PageSize);
        }
    }
}