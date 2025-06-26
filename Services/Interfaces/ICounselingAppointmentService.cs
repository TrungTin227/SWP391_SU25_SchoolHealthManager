using DTOs.CounselingAppointmentDTOs.Requests;
using DTOs.CounselingAppointmentDTOs.Responds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICounselingAppointmentService
    {
        Task<ApiResult<CounselingAppointmentRespondDTO>> CreateCounselingAppointmentAsync(CreateCounselingAppointmentRequestDTO request);
        Task<ApiResult<AddNoteAndRecommendRequestDTO>> AddNoteAndRecommend(AddNoteAndRecommendRequestDTO request);
        Task<ApiResult<bool>> StartAppointment(Guid AppointmentId);
        Task<ApiResult<CounselingAppointmentRespondDTO>> UpdateAppointmentAsync(UpdateCounselingAppointmentRequestDTO request);
        Task<ApiResult<CounselingAppointmentRespondDTO?>> GetByIdAsync(Guid id);
        Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllByStaffIdAsync(Guid id);
        Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllPendingByStaffIdAsync(Guid id);
        Task<ApiResult<List<CounselingAppointmentRespondDTO?>>> GetAllByStudentCodeAsync(string studentId);
        Task<ApiResult<bool>> SoftDeleteAsync(Guid id);
        Task<ApiResult<bool>> SoftDeleteRangeAsync(List<Guid> id);

        //Task<ApiResult<bool>> AcceptAppointmentAsync(Guid appointmentId, Guid counselorId);
        //Task<ApiResult<bool>> RejectAppointmentAsync(Guid appointmentId, Guid counselorId);
    }
}
