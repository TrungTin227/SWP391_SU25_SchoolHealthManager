using Azure;
using BusinessObjects;
using DTOs.CheckUpRecordDTOs.Requests;
using DTOs.CheckUpRecordDTOs.Responds;
using DTOs.CounselingAppointmentDTOs.Requests;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implementations
{
    public class CheckupRecordService : BaseService<CheckupRecordService, Guid>, ICheckupRecordService
    {
        private readonly ICounselingAppointmentService _counselingAppointmentService;
        private readonly ILogger<CheckupRecordService> _logger;
        public CheckupRecordService(
            IGenericRepository<CheckupRecordService, Guid> repository
            , ICurrentUserService currentUserService, 
            IUnitOfWork unitOfWork, 
            ICurrentTime currentTime,
            ICounselingAppointmentService counselingAppointmentService,
            ILogger<CheckupRecordService> logger) : 
            base(repository, 
                currentUserService, 
                unitOfWork, 
                currentTime)
        {
            _counselingAppointmentService = counselingAppointmentService;
            _logger = logger;
        }

        //public async Task<ApiResult<CheckupRecordRespondDTO>> CreateCheckupRecordAsync(CreateCheckupRecordRequestDTO request)
        //{
        //    await _unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        var schedule = await _unitOfWork.CheckupScheduleRepository.GetByIdAsync(request.ScheduleId);
        //        if (schedule == null)
        //        {
        //            return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Lịch khám không tồn tại!!"));
        //        }   

        //        // 1. Tạo CheckupRecord
        //        var checkupRecord = CheckupRecordMappings.MapToEntity(request);

        //        await _unitOfWork.CheckupRecordRepository.AddAsync(checkupRecord);
        //        await _unitOfWork.SaveChangesAsync();

        //        // 2. Nếu cần khám lại → gọi service tạo CounselingAppointment
        //        if (request.Status == CheckupRecordStatus.RequiresFollowUp)
        //        {
        //            foreach(var counselingAppointment in request.CounselingAppointment)
        //            {
        //                await _counselingAppointmentService.CreateCounselingAppointmentAsync(counselingAppointment);
        //            }
        //            //var counselingRequest = new CreateCounselingAppointmentRequestDTO
        //            //{
        //            //    CheckupRecordId = checkupRecord.Id,
        //            //    StudentId = request.CounselingAppointment.,
        //            //    ParentId = request.CounselingAppointment.ParentId,
        //            //    StaffUserId = request.CounselingAppointment.StaffUserId,
        //            //    AppointmentDate = request.CounselingAppointment.AppointmentDate,
        //            //    Duration = request.CounselingAppointment.Duration,
        //            //    Purpose = request.CounselingAppointment.Purpose,
        //            //    VaccinationRecordId = request.CounselingAppointment.VaccinationRecordId,
        //            //};

        //            //await _counselingAppointmentService.CreateCounselingAppointmentAsync(counselingRequest);
        //        }

        //        await _unitOfWork.CommitTransactionAsync();
        //        var response = CheckupRecordMappings.MapToRespondDTO(checkupRecord);
        //        return ApiResult<CheckupRecordRespondDTO>.Success(response, "Tạo hồ sơ kiểm tra thành công!!");
        //    }
        //    catch (Exception)
        //    {
        //        await _unitOfWork.RollbackTransactionAsync();
        //        return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo hồ sơ kiểm tra thất bại!!"));
        //        throw;
        //    }
        //}
        public async Task<ApiResult<CheckupRecordRespondDTO>> CreateCheckupRecordAsync(CreateCheckupRecordRequestDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra lịch khám tồn tại
                var schedule = await _unitOfWork.CheckupScheduleRepository.GetByIdAsync(request.ScheduleId);
                if (schedule == null)
                {
                    return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Lịch khám không tồn tại!!"));
                }

                // 2. Tạo CheckupRecord
                var checkupRecord = CheckupRecordMappings.MapToEntity(request);
                checkupRecord.Id = Guid.NewGuid(); // đảm bảo có ID trước khi dùng
                await _unitOfWork.CheckupRecordRepository.AddAsync(checkupRecord);

                //await _unitOfWork.SaveChangesAsync(); // lưu ngay để có ID

                // 3. Nếu cần tư vấn → tạo CounselingAppointment
                if (request.Status == CheckupRecordStatus.RequiresFollowUp &&
                    request.CounselingAppointment != null &&
                    request.CounselingAppointment.Any())
                {
                    foreach (var caDto in request.CounselingAppointment)
                    {
                        // Clone DTO để tránh reference bug
                        var appointmentDto = new CreateCounselingAppointmentRequestDTO
                        {
                            StudentId = caDto.StudentId,
                            ParentId = caDto.ParentId,
                            StaffUserId = caDto.StaffUserId,
                            AppointmentDate = caDto.AppointmentDate,
                            Duration = caDto.Duration,
                            Purpose = caDto.Purpose,
                            VaccinationRecordId = caDto.VaccinationRecordId,
                            CheckupRecordId = checkupRecord.Id
                        };

                        _logger.LogWarning("🐛 Tạo entity với Purpose = {Purpose}", appointmentDto.Purpose);


                        var result = await _counselingAppointmentService.CreateCounselingAppointmentAsync(appointmentDto);

                        if (!result.IsSuccess)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo appointment thất bại: " + result.Message));
                        }
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
                var response = CheckupRecordMappings.MapToRespondDTO(checkupRecord);
                return ApiResult<CheckupRecordRespondDTO>.Success(response, "Tạo hồ sơ kiểm tra thành công!!");
            }
            catch (Exception e)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResult<CheckupRecordRespondDTO>.Failure(new Exception("Tạo hồ sơ kiểm tra thất bại!! " + e.Message));
            }
        }


    }
}
