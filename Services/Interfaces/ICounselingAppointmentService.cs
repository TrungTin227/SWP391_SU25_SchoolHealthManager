using DTOs.CounselingAppointmentDTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ICounselingAppointmentService
    {
        Task<ApiResult<CreateCounselingAppointmentRequestDTO>> CreateCounselingAppointmentAsync(CreateCounselingAppointmentRequestDTO request);
        Task<ApiResult<AddNoteAndRecommendRequestDTO>> AddNoteAndRecommend(AddNoteAndRecommendRequestDTO request);
        //Task<ApiResult<UpdateCounselingAppointmentRequestDTO>> UpdateAsync(UpdateCounselingAppointmentRequestDTO request);
        //Task<ApiResult<List<GetCounselingAppointmentRespondDTO>>> GetAllAsync();
        //Task<ApiResult<GetCounselingAppointmentRespondDTO?>> GetByIdAsync(Guid id);
        //Task<ApiResult<List<GetCounselingAppointmentRespondDTO>>> GetAllByStudentIdAsync(Guid studentId);
        //Task<ApiResult<bool>> DeleteAsync(Guid id);
        //Task<ApiResult<bool>> AcceptAppointmentAsync(Guid appointmentId, Guid counselorId);
        //Task<ApiResult<bool>> RejectAppointmentAsync(Guid appointmentId, Guid counselorId);
    }
}
