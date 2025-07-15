namespace Services.Helpers.Mappers
{
    public static class CheckupScheduleMapper
    {
        public static CheckupScheduleForParentResponseDTO MapToParentDTO(CheckupSchedule schedule)
        {
            return new CheckupScheduleForParentResponseDTO
            {
                Id = schedule.Id,
                ScheduledAt = schedule.ScheduledAt,
                CampaignName = schedule.Campaign?.Name ?? string.Empty,
                StudentName = schedule.Student?.FullName ?? string.Empty,
                ParentConsentStatus = schedule.ParentConsentStatus
            };
        }
    }

}
