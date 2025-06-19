using DTOs.VaccinationScheduleDTOs.Request;
using DTOs.VaccinationScheduleDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;

namespace Services.Implementations
{
    public class VaccinationScheduleService : BaseService<VaccinationSchedule, Guid>, IVaccinationScheduleService
    {
        private readonly IVaccinationScheduleRepository _scheduleRepository;
        private readonly ILogger<VaccinationScheduleService> _logger;

        public VaccinationScheduleService(
            IVaccinationScheduleRepository scheduleRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ILogger<VaccinationScheduleService> logger)
            : base(scheduleRepository, currentUserService, unitOfWork, currentTime)
        {
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<ApiResult<VaccinationScheduleDetailResponseDTO>> CreateScheduleAsync(CreateVaccinationScheduleRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate campaign exists
                    var campaign = await _unitOfWork.VaccinationCampaignRepository.GetByIdAsync(request.CampaignId);
                    if (campaign == null)
                    {
                        return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy chiến dịch với ID: {request.CampaignId}"));
                    }

                    // Validate vaccination type exists
                    var vaccinationType = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(request.VaccinationTypeId);
                    if (vaccinationType == null)
                    {
                        return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy loại vắc-xin với ID: {request.VaccinationTypeId}"));
                    }

                    // Check for conflicts
                    var hasConflict = await _scheduleRepository.IsScheduleConflictAsync(request.CampaignId, request.ScheduledAt);
                    if (hasConflict)
                    {
                        return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                            new InvalidOperationException("Đã có lịch tiêm khác trong cùng ngày cho chiến dịch này"));
                    }

                    var schedule = new VaccinationSchedule
                    {
                        Id = Guid.NewGuid(),
                        CampaignId = request.CampaignId,
                        VaccinationTypeId = request.VaccinationTypeId,
                        ScheduledAt = request.ScheduledAt,
                        ScheduleStatus = ScheduleStatus.Pending
                    };

                    var createdSchedule = await CreateAsync(schedule);

                    // Add students to schedule
                    if (request.StudentIds.Any())
                    {
                        var currentUserId = GetCurrentUserIdOrThrow();
                        await _scheduleRepository.AddStudentsToScheduleAsync(createdSchedule.Id, request.StudentIds, currentUserId);
                    }

                    var scheduleWithDetails = await _scheduleRepository.GetScheduleWithDetailsAsync(createdSchedule.Id);
                    var response = VaccinationScheduleMapper.MapToDetailResponseDTO(scheduleWithDetails!);

                    _logger.LogInformation("Tạo lịch tiêm thành công: {ScheduleId}", createdSchedule.Id);
                    return ApiResult<VaccinationScheduleDetailResponseDTO>.Success(response, "Tạo lịch tiêm thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo lịch tiêm");
                    return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationScheduleDetailResponseDTO>> UpdateScheduleAsync(Guid id, UpdateVaccinationScheduleRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var schedule = await _scheduleRepository.GetScheduleWithDetailsAsync(id);
                    if (schedule == null)
                    {
                        return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                            new KeyNotFoundException($"Không tìm thấy lịch tiêm với ID: {id}"));
                    }

                    // Check if schedule can be updated
                    if (schedule.ScheduleStatus == ScheduleStatus.Completed)
                    {
                        return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                            new InvalidOperationException("Không thể cập nhật lịch tiêm đã hoàn thành"));
                    }

                    // Update fields
                    if (request.VaccinationTypeId.HasValue)
                    {
                        var vaccinationType = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(request.VaccinationTypeId.Value);
                        if (vaccinationType == null)
                        {
                            return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                                new KeyNotFoundException($"Không tìm thấy loại vắc-xin với ID: {request.VaccinationTypeId}"));
                        }
                        schedule.VaccinationTypeId = request.VaccinationTypeId.Value;
                    }

                    if (request.ScheduledAt.HasValue)
                    {
                        var hasConflict = await _scheduleRepository.IsScheduleConflictAsync(
                            schedule.CampaignId, request.ScheduledAt.Value, id);
                        if (hasConflict)
                        {
                            return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                                new InvalidOperationException("Đã có lịch tiêm khác trong cùng ngày cho chiến dịch này"));
                        }
                        schedule.ScheduledAt = request.ScheduledAt.Value;
                    }

                    // Update students if provided
                    if (request.StudentIds != null)
                    {
                        var currentStudentIds = schedule.SessionStudents.Select(ss => ss.StudentId).ToList();
                        var studentsToRemove = currentStudentIds.Except(request.StudentIds).ToList();
                        var studentsToAdd = request.StudentIds.Except(currentStudentIds).ToList();

                        if (studentsToRemove.Any())
                        {
                            await _scheduleRepository.RemoveStudentsFromScheduleAsync(id, studentsToRemove);
                        }

                        if (studentsToAdd.Any())
                        {
                            var currentUserId = GetCurrentUserIdOrThrow();
                            await _scheduleRepository.AddStudentsToScheduleAsync(id, studentsToAdd, currentUserId);
                        }
                    }

                    var updatedSchedule = await UpdateAsync(schedule);
                    var scheduleWithDetails = await _scheduleRepository.GetScheduleWithDetailsAsync(updatedSchedule.Id);
                    var response = VaccinationScheduleMapper.MapToDetailResponseDTO(scheduleWithDetails!);

                    _logger.LogInformation("Cập nhật lịch tiêm thành công: {ScheduleId}", id);
                    return ApiResult<VaccinationScheduleDetailResponseDTO>.Success(response, "Cập nhật lịch tiêm thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật lịch tiêm với ID: {Id}", id);
                    return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationScheduleDetailResponseDTO>> GetScheduleByIdAsync(Guid id)
        {
            try
            {
                var schedule = await _scheduleRepository.GetScheduleWithDetailsAsync(id);
                if (schedule == null)
                {
                    return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(
                        new KeyNotFoundException($"Không tìm thấy lịch tiêm với ID: {id}"));
                }

                var response = VaccinationScheduleMapper.MapToDetailResponseDTO(schedule);
                return ApiResult<VaccinationScheduleDetailResponseDTO>.Success(response, "Lấy thông tin lịch tiêm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin lịch tiêm với ID: {Id}", id);
                return ApiResult<VaccinationScheduleDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesByCampaignAsync(
            Guid campaignId, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var schedules = await _scheduleRepository.GetSchedulesByCampaignAsync(
                    campaignId, pageNumber, pageSize, searchTerm);

                var response = VaccinationScheduleMapper.ToPagedResponseDTO(schedules);
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Success(
                    response, $"Lấy danh sách lịch tiêm thành công. Tổng số: {response.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm theo chiến dịch");
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesByDateRangeAsync(
            DateTime startDate, DateTime endDate, int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var schedules = await _scheduleRepository.GetSchedulesByDateRangeAsync(
                    startDate, endDate, pageNumber, pageSize, searchTerm);

                var response = VaccinationScheduleMapper.ToPagedResponseDTO(schedules);
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Success(
                    response, $"Lấy danh sách lịch tiêm thành công. Tổng số: {response.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm theo khoảng thời gian");
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Student Management

        public async Task<ApiResult<bool>> AddStudentsToScheduleAsync(Guid scheduleId, List<Guid> studentIds)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                    if (schedule == null)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException($"Không tìm thấy lịch tiêm với ID: {scheduleId}"));
                    }

                    if (schedule.ScheduleStatus == ScheduleStatus.Completed)
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException("Không thể thêm học sinh vào lịch tiêm đã hoàn thành"));
                    }

                    var currentUserId = GetCurrentUserIdOrThrow();
                    var result = await _scheduleRepository.AddStudentsToScheduleAsync(scheduleId, studentIds, currentUserId);

                    if (result)
                    {
                        _logger.LogInformation("Thêm {Count} học sinh vào lịch tiêm {ScheduleId}", studentIds.Count, scheduleId);
                        return ApiResult<bool>.Success(true, "Thêm học sinh vào lịch tiêm thành công");
                    }

                    return ApiResult<bool>.Failure(new InvalidOperationException("Không thể thêm học sinh vào lịch tiêm"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thêm học sinh vào lịch tiêm {ScheduleId}", scheduleId);
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<bool>> RemoveStudentsFromScheduleAsync(Guid scheduleId, List<Guid> studentIds)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                    if (schedule == null)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException($"Không tìm thấy lịch tiêm với ID: {scheduleId}"));
                    }

                    if (schedule.ScheduleStatus == ScheduleStatus.Completed)
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException("Không thể xóa học sinh khỏi lịch tiêm đã hoàn thành"));
                    }

                    var result = await _scheduleRepository.RemoveStudentsFromScheduleAsync(scheduleId, studentIds);

                    if (result)
                    {
                        _logger.LogInformation("Xóa {Count} học sinh khỏi lịch tiêm {ScheduleId}", studentIds.Count, scheduleId);
                        return ApiResult<bool>.Success(true, "Xóa học sinh khỏi lịch tiêm thành công");
                    }

                    return ApiResult<bool>.Failure(new InvalidOperationException("Không thể xóa học sinh khỏi lịch tiêm"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi xóa học sinh khỏi lịch tiêm {ScheduleId}", scheduleId);
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        #endregion

        #region Status Management

        public async Task<ApiResult<bool>> UpdateScheduleStatusAsync(Guid scheduleId, ScheduleStatus newStatus)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                    if (schedule == null)
                    {
                        return ApiResult<bool>.Failure(
                            new KeyNotFoundException($"Không tìm thấy lịch tiêm với ID: {scheduleId}"));
                    }

                    // Validate status transition
                    if (!IsValidStatusTransition(schedule.ScheduleStatus, newStatus))
                    {
                        return ApiResult<bool>.Failure(
                            new InvalidOperationException($"Không thể chuyển trạng thái từ {schedule.ScheduleStatus} sang {newStatus}"));
                    }

                    var currentUserId = GetCurrentUserIdOrThrow();
                    var result = await _scheduleRepository.UpdateScheduleStatusAsync(scheduleId, newStatus, currentUserId);

                    if (result)
                    {
                        _logger.LogInformation("Cập nhật trạng thái lịch tiêm {ScheduleId} thành {Status}", scheduleId, newStatus);
                        return ApiResult<bool>.Success(true, $"Cập nhật trạng thái lịch tiêm thành {newStatus} thành công");
                    }

                    return ApiResult<bool>.Failure(new InvalidOperationException("Không thể cập nhật trạng thái lịch tiêm"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật trạng thái lịch tiêm {ScheduleId}", scheduleId);
                    return ApiResult<bool>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> BatchUpdateScheduleStatusAsync(BatchUpdateScheduleStatusRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = request.ScheduleIds.Count
                };

                try
                {
                    var schedules = await _scheduleRepository.GetSchedulesByIdsAsync(request.ScheduleIds);
                    var foundIds = schedules.Select(s => s.Id).ToHashSet();

                    foreach (var scheduleId in request.ScheduleIds)
                    {
                        try
                        {
                            if (!foundIds.Contains(scheduleId))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = scheduleId.ToString(),
                                    Error = "Không tìm thấy",
                                    Details = $"Lịch tiêm với ID {scheduleId} không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var schedule = schedules.First(s => s.Id == scheduleId);

                            if (!IsValidStatusTransition(schedule.ScheduleStatus, request.NewStatus))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = scheduleId.ToString(),
                                    Error = "Trạng thái không hợp lệ",
                                    Details = $"Không thể chuyển từ {schedule.ScheduleStatus} sang {request.NewStatus}"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var currentUserId = GetCurrentUserIdOrThrow();
                            var updateResult = await _scheduleRepository.UpdateScheduleStatusAsync(scheduleId, request.NewStatus, currentUserId);

                            if (updateResult)
                            {
                                result.SuccessIds.Add(scheduleId.ToString());
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = scheduleId.ToString(),
                                    Error = "Cập nhật thất bại",
                                    Details = "Không thể cập nhật trạng thái"
                                });
                                result.FailureCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = scheduleId.ToString(),
                                Error = "Lỗi hệ thống",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("cập nhật trạng thái", result);
                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật hàng loạt trạng thái lịch tiêm");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteSchedulesAsync(List<Guid> ids, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = ids.Count
                };

                try
                {
                    var schedules = await _scheduleRepository.GetSchedulesByIdsAsync(ids, includeDeleted: isPermanent);
                    var foundIds = schedules.Select(s => s.Id).ToHashSet();

                    foreach (var id in ids)
                    {
                        try
                        {
                            if (!foundIds.Contains(id))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Không tìm thấy",
                                    Details = $"Lịch tiêm với ID {id} không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var schedule = schedules.First(s => s.Id == id);

                            if (isPermanent)
                            {
                                var canDelete = await _scheduleRepository.CanDeleteScheduleAsync(id);
                                if (!canDelete)
                                {
                                    result.Errors.Add(new BatchOperationErrorDTO
                                    {
                                        Id = id.ToString(),
                                        Error = "Có ràng buộc dữ liệu",
                                        Details = "Lịch tiêm đã có bản ghi tiêm chủng"
                                    });
                                    result.FailureCount++;
                                    continue;
                                }

                                await _repository.DeleteAsync(id);
                            }
                            else
                            {
                                await DeleteAsync(id);
                            }

                            result.SuccessIds.Add(id.ToString());
                            result.SuccessCount++;
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Lỗi hệ thống",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage(isPermanent ? "xóa vĩnh viễn" : "xóa", result);
                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi thực hiện batch delete lịch tiêm");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreSchedulesAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var result = new BatchOperationResultDTO
                {
                    TotalRequested = ids.Count
                };

                try
                {
                    var schedules = await _scheduleRepository.GetSchedulesByIdsAsync(ids, includeDeleted: true);
                    var foundIds = schedules.Select(s => s.Id).ToHashSet();

                    foreach (var id in ids)
                    {
                        try
                        {
                            if (!foundIds.Contains(id))
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Không tìm thấy",
                                    Details = $"Lịch tiêm với ID {id} không tồn tại"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var schedule = schedules.First(s => s.Id == id);

                            if (!schedule.IsDeleted)
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Trạng thái không hợp lệ",
                                    Details = "Lịch tiêm chưa bị xóa"
                                });
                                result.FailureCount++;
                                continue;
                            }

                            var currentUserId = GetCurrentUserIdOrThrow();
                            var restoreResult = await _scheduleRepository.RestoreScheduleAsync(id, currentUserId);

                            if (restoreResult)
                            {
                                result.SuccessIds.Add(id.ToString());
                                result.SuccessCount++;
                            }
                            else
                            {
                                result.Errors.Add(new BatchOperationErrorDTO
                                {
                                    Id = id.ToString(),
                                    Error = "Khôi phục thất bại",
                                    Details = "Không thể khôi phục lịch tiêm"
                                });
                                result.FailureCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(new BatchOperationErrorDTO
                            {
                                Id = id.ToString(),
                                Error = "Lỗi hệ thống",
                                Details = ex.Message
                            });
                            result.FailureCount++;
                        }
                    }

                    result.Message = GenerateBatchOperationMessage("khôi phục", result);
                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi khôi phục hàng loạt lịch tiêm");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSoftDeletedSchedulesAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var schedules = await _scheduleRepository.GetSoftDeletedSchedulesAsync(
                    pageNumber, pageSize, searchTerm);

                var response = VaccinationScheduleMapper.ToPagedResponseDTO(schedules);
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Success(
                    response, "Lấy danh sách lịch tiêm đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm đã xóa");
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Business Operations

        public async Task<ApiResult<List<VaccinationScheduleResponseDTO>>> GetPendingSchedulesAsync()
        {
            try
            {
                var schedules = await _scheduleRepository.GetSchedulesByStatusAsync(ScheduleStatus.Pending);
                var response = schedules.Select(VaccinationScheduleMapper.MapToResponseDTO).ToList();

                return ApiResult<List<VaccinationScheduleResponseDTO>>.Success(
                    response, $"Lấy danh sách lịch tiêm chờ xử lý thành công. Tổng số: {response.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm chờ xử lý");
                return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<List<VaccinationScheduleResponseDTO>>> GetInProgressSchedulesAsync()
        {
            try
            {
                var schedules = await _scheduleRepository.GetSchedulesByStatusAsync(ScheduleStatus.InProgress);
                var response = schedules.Select(VaccinationScheduleMapper.MapToResponseDTO).ToList();

                return ApiResult<List<VaccinationScheduleResponseDTO>>.Success(
                    response, $"Lấy danh sách lịch tiêm đang thực hiện thành công. Tổng số: {response.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch tiêm đang thực hiện");
                return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<bool>> StartScheduleAsync(Guid scheduleId)
        {
            return await UpdateScheduleStatusAsync(scheduleId, ScheduleStatus.InProgress);
        }

        public async Task<ApiResult<bool>> CompleteScheduleAsync(Guid scheduleId)
        {
            return await UpdateScheduleStatusAsync(scheduleId, ScheduleStatus.Completed);
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Lấy current user ID và ném exception nếu null
        /// </summary>
        /// <returns>Guid của current user</returns>
        /// <exception cref="UnauthorizedAccessException">Khi không có user đăng nhập</exception>
        private Guid GetCurrentUserIdOrThrow()
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng hiện tại");
            }
            return currentUserId.Value;
        }

        private static bool IsValidStatusTransition(ScheduleStatus currentStatus, ScheduleStatus newStatus)
        {
            return currentStatus switch
            {
                ScheduleStatus.Pending => newStatus is ScheduleStatus.InProgress or ScheduleStatus.Cancelled,
                ScheduleStatus.InProgress => newStatus is ScheduleStatus.Completed or ScheduleStatus.Cancelled,
                ScheduleStatus.Completed => false, // Cannot change from completed
                ScheduleStatus.Cancelled => newStatus == ScheduleStatus.Pending, // Can restart cancelled
                _ => false
            };
        }

        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
                return $"Đã {operation} thành công {result.SuccessCount} lịch tiêm";

            if (result.IsPartialSuccess)
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} lịch tiêm. " +
                       $"{result.FailureCount} thất bại";

            return $"Không thể {operation} lịch tiêm nào. Tất cả {result.FailureCount} yêu cầu đều thất bại";
        }

        #endregion
    }
}