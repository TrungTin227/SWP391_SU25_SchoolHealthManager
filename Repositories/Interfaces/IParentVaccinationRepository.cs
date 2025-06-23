using BusinessObjects;
using BusinessObjects.Common;
using DTOs.ParentVaccinationDTOs.Response;

namespace Repositories.Interfaces
{
    public interface IParentVaccinationRepository
    {
        // Lấy lịch tiêm của phụ huynh theo trạng thái
        Task<PagedList<VaccinationSchedule>> GetParentVaccinationSchedulesAsync(
            Guid parentUserId, ParentActionStatus? status, int pageNumber, int pageSize);

        // Lấy session students của phụ huynh
        Task<List<SessionStudent>> GetParentSessionStudentsAsync(
            Guid parentUserId, Guid scheduleId);

        // Lấy lịch sử tiêm chủng của tất cả con
        Task<List<VaccinationRecord>> GetParentVaccinationHistoryAsync(Guid parentUserId);

        // Lấy lịch sử tiêm chủng của một học sinh
        Task<List<VaccinationRecord>> GetStudentVaccinationHistoryAsync(
            Guid parentUserId, Guid studentId);

        // Kiểm tra quyền truy cập
        Task<bool> CanParentAccessStudentAsync(Guid parentUserId, Guid studentId);
        Task<bool> CanParentAccessSessionAsync(Guid parentUserId, Guid sessionStudentId);

        // Thống kê
        Task<Dictionary<ParentActionStatus, int>> GetParentVaccinationStatsAsync(Guid parentUserId);

        // Lấy các lịch tiêm cần theo dõi
        Task<List<VaccinationRecord>> GetFollowUpVaccinationsAsync(Guid parentUserId);
    }
}