using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DTOs.VaccinationRecordDTOs.Request;
using DTOs.VaccinationRecordDTOs.Response;
using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccinationRecordService
        : BaseService<VaccinationRecord, Guid>, IVaccinationRecordService
    {
        private readonly IVaccinationRecordRepository _recordRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<VaccinationRecordService> _logger;

        public VaccinationRecordService(
            IVaccinationRecordRepository recordRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            IMapper mapper,
            ILogger<VaccinationRecordService> logger)
            : base(recordRepository, currentUserService, unitOfWork, currentTime)
        {
            _recordRepository = recordRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region CRUD

        public async Task<ApiResult<CreateVaccinationRecordResponse>> CreateAsync(CreateVaccinationRecordRequest request)
        {
            try
            {
                var exists = await _recordRepository.HasDuplicateEntryAsync(request.StudentId, request.ScheduleId);
                if (exists)
                {
                    return ApiResult<CreateVaccinationRecordResponse>.Failure(new InvalidOperationException("Học sinh đã có phiếu tiêm trong lịch này."));
                }

                var record = _mapper.Map<VaccinationRecord>(request);
                record.Id = Guid.NewGuid();
                record.CreatedAt = _currentTime.GetCurrentTime();
                record.UpdatedAt = _currentTime.GetCurrentTime();
                record.IsDeleted = false;

                var created = await base.CreateAsync(record);
                var response = _mapper.Map<CreateVaccinationRecordResponse>(created);

                return ApiResult<CreateVaccinationRecordResponse>.Success(response, "Tạo phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phiếu tiêm");
                return ApiResult<CreateVaccinationRecordResponse>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> UpdateAsync(Guid id, UpdateVaccinationRecordRequest request)
        {
            try
            {
                var record = await _recordRepository.GetByIdAsync(id);
                if (record == null || record.IsDeleted)
                {
                    return ApiResult<bool>.Failure(
                        new KeyNotFoundException("Không tìm thấy phiếu tiêm."));
                }

                _mapper.Map(request, record);
                record.UpdatedAt = _currentTime.GetCurrentTime();

                var updated = await base.UpdateAsync(record);
                if (updated != null)
                {
                    _logger.LogInformation("Cập nhật phiếu tiêm thành công: {RecordId}", id);
                    return ApiResult<bool>.Success(true, "Cập nhật phiếu tiêm thành công");
                }

                return ApiResult<bool>.Failure(
                    new InvalidOperationException("Không thể cập nhật phiếu tiêm."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu tiêm {RecordId}", id);
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> DeleteAsync(Guid id, Guid deletedBy)
        {
            try
            {
                var record = await _recordRepository.GetByIdAsync(id);
                if (record == null || record.IsDeleted)
                    return ApiResult<bool>.Failure(new Exception("Không tìm thấy phiếu tiêm."));

                record.IsDeleted = true;
                record.DeletedAt = _currentTime.GetCurrentTime();
                record.DeletedBy = deletedBy;

                var deleted = await base.UpdateAsync(record);
                if (deleted != null)
                {
                    _logger.LogInformation("Xóa mềm phiếu tiêm thành công: {RecordId}", id);
                    return ApiResult<bool>.Success(true, "Xóa phiếu tiêm thành công");
                }

                return ApiResult<bool>.Failure(new InvalidOperationException("Không thể xóa phiếu tiêm."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm phiếu tiêm");
                return ApiResult<bool>.Failure(ex);
            }
        }

        public async Task<ApiResult<VaccinationRecord?>> GetByIdAsync(Guid id)
        {
            try
            {
                var record = await _recordRepository.GetRecordWithDetailsAsync(id);
                return ApiResult<VaccinationRecord?>.Success(record, "Lấy phiếu tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phiếu tiêm theo ID");
                return ApiResult<VaccinationRecord?>.Failure(ex);
            }
        }

        #endregion

        #region Queries

        public async Task<ApiResult<PagedList<VaccinationRecord>>> GetRecordsByScheduleAsync(Guid scheduleId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByScheduleAsync(scheduleId, pageNumber, pageSize, searchTerm);
                return ApiResult<PagedList<VaccinationRecord>>.Success(records, "Lấy danh sách phiếu tiêm theo lịch thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo lịch");
                return ApiResult<PagedList<VaccinationRecord>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<VaccinationRecord>>> GetRecordsByStudentAsync(Guid studentId)
        {
            try
            {
                var records = await _recordRepository.GetRecordsByStudentAsync(studentId);
                return ApiResult<List<VaccinationRecord>>.Success(records, "Lấy danh sách phiếu tiêm theo học sinh thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phiếu tiêm theo học sinh");
                return ApiResult<List<VaccinationRecord>>.Failure(ex);
            }
        }

        #endregion
    }
}
