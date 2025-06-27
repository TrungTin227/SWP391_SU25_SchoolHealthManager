using Microsoft.Extensions.Logging;

namespace Services.Implementations
{
    public class VaccinationScheduleService : BaseService<VaccinationSchedule, Guid>, IVaccinationScheduleService
    {
        private readonly ILogger<VaccinationScheduleService> _logger;
        private readonly ISessionStudentService _sessionStudent;

        public VaccinationScheduleService(
            IVaccinationScheduleRepository scheduleRepository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            ILogger<VaccinationScheduleService> logger,
            ISessionStudentService sessionStudent)
            : base(scheduleRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger;
            _sessionStudent = sessionStudent;
        }

        #region CRUD Operations

        public async Task<ApiResult<PagedList<VaccinationScheduleResponseDTO>>> GetSchedulesAsync(
            Guid? campaignId,
            DateTime? startDate,
            DateTime? endDate,
            ScheduleStatus? status,
            string? searchTerm,
            int pageNumber,
            int pageSize)
        {
            try
            {
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesAsync(
                    campaignId,
                    startDate,
                    endDate,
                    status,
                    searchTerm,
                    pageNumber,
                    pageSize);

                var response = VaccinationScheduleMapper.ToPagedResponseDTO(schedules);
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Success(
                    response,
                    $"Lấy lịch tiêm thành công. Tổng: {response.MetaData.TotalCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving filtered schedules");
                return ApiResult<PagedList<VaccinationScheduleResponseDTO>>.Failure(
                    new Exception("Đã xảy ra lỗi khi lấy danh sách lịch tiêm"));
            }
        }

        public async Task<ApiResult<List<VaccinationScheduleResponseDTO>>> CreateSchedulesAsync(CreateVaccinationScheduleRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // Validate input
                    if (!HasValidSelection(request))
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new InvalidOperationException("Phải chọn ít nhất một trong các tùy chọn: StudentIds, Grades, Sections, hoặc IncludeAllStudentsInGrades"));
                    }

                    // Validate campaign exists
                    var campaign = await _unitOfWork.VaccinationCampaignRepository.GetByIdAsync(request.CampaignId);
                    if (campaign == null)
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new KeyNotFoundException($"Không tìm thấy chiến dịch với ID: {request.CampaignId}"));
                    }

                    // Validate vaccination type exists
                    var vaccinationType = await _unitOfWork.VaccineTypeRepository.GetByIdAsync(request.VaccinationTypeId);
                    if (vaccinationType == null)
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new KeyNotFoundException($"Không tìm thấy loại vắc-xin với ID: {request.VaccinationTypeId}"));
                    }

                    // Get student IDs from request
                    var studentIds = await GetStudentIdsFromRequest(request);
                    if (!studentIds.Any())
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new InvalidOperationException("Không tìm thấy học sinh nào phù hợp với tiêu chí đã chọn"));
                    }

                    // Validate all student IDs exist
                    var validStudents = await _unitOfWork.StudentRepository.GetStudentsByIdsAsync(studentIds.ToList());
                    if (validStudents.Count != studentIds.Count)
                    {
                        var invalidIds = studentIds.Except(validStudents.Select(s => s.Id)).ToList();
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new InvalidOperationException($"Các học sinh không tồn tại: {string.Join(", ", invalidIds)}"));
                    }

                    // Check for scheduling conflicts
                    var hasConflict = await _unitOfWork.VaccinationScheduleRepository.IsScheduleConflictAsync(
                        request.CampaignId, request.ScheduledAt);
                    if (hasConflict)
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new InvalidOperationException("Đã có lịch tiêm khác trong cùng ngày cho chiến dịch này"));
                    }

                    // Create schedules from student IDs (sẽ trả về 1 schedule với nhiều SessionStudent)
                    var schedules = await CreateSchedulesFromStudentIds(request, studentIds);
                    if (!schedules.Any())
                    {
                        return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(
                            new InvalidOperationException("Không thể tạo lịch tiêm do xung đột thời gian"));
                    }

                    // Save schedules to database using BaseService
                    var createdSchedules = new List<VaccinationSchedule>();
                    foreach (var schedule in schedules)
                    {
                        // BaseService will handle audit fields automatically for the schedule and SessionStudents
                        var createdSchedule = await CreateAsync(schedule);
                        createdSchedules.Add(createdSchedule);

                        _logger.LogInformation("Tạo lịch tiêm {ScheduleId} với {StudentCount} học sinh",
                            createdSchedule.Id, schedule.SessionStudents.Count);
                    }

                    // Map to response DTOs
                    var responseDTOs = createdSchedules.Select(VaccinationScheduleMapper.MapToResponseDTO).ToList();

                    var totalStudents = createdSchedules.Sum(s => s.SessionStudents?.Count ?? 0);
                    _logger.LogInformation("Tạo thành công {ScheduleCount} lịch tiêm với tổng {StudentCount} học sinh cho chiến dịch {CampaignId}",
                        responseDTOs.Count, totalStudents, request.CampaignId);
                    // ✅ Gửi email thông báo cho phụ huynh
                    foreach (var createdSchedule in createdSchedules)
                    {
                        var studentIdsToNotify = createdSchedule.SessionStudents
                            .Select(ss => ss.StudentId)
                            .ToList();

                        await _sessionStudent.SendVaccinationNotificationEmailToParents(studentIdsToNotify, vaccinationType.Name, createdSchedule);
                    }
                    return ApiResult<List<VaccinationScheduleResponseDTO>>.Success(responseDTOs,
                        $"Tạo thành công {responseDTOs.Count} lịch tiêm với {totalStudents} học sinh");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo lịch tiêm hàng loạt");
                    return ApiResult<List<VaccinationScheduleResponseDTO>>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<VaccinationScheduleDetailResponseDTO>> UpdateScheduleAsync(Guid id, UpdateVaccinationScheduleRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var schedule = await _unitOfWork.VaccinationScheduleRepository.GetScheduleWithDetailsAsync(id);
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
                        var hasConflict = await _unitOfWork.VaccinationScheduleRepository.IsScheduleConflictAsync(
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
                            await _unitOfWork.VaccinationScheduleRepository.RemoveStudentsFromScheduleAsync(id, studentsToRemove);
                        }

                        if (studentsToAdd.Any())
                        {
                            await _unitOfWork.VaccinationScheduleRepository.AddStudentsToScheduleAsync(id, studentsToAdd, GetCurrentUserIdOrThrow());
                        }
                    }

                    // Use BaseService UpdateAsync - it will handle audit fields automatically
                    var updatedSchedule = await UpdateAsync(schedule);
                    var scheduleWithDetails = await _unitOfWork.VaccinationScheduleRepository.GetScheduleWithDetailsAsync(updatedSchedule.Id);
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
                var schedule = await _unitOfWork.VaccinationScheduleRepository.GetScheduleWithDetailsAsync(id);
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
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByCampaignAsync(
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
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByDateRangeAsync(
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
                    var schedule = await _unitOfWork.VaccinationScheduleRepository.GetByIdAsync(scheduleId);
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

                    var result = await _unitOfWork.VaccinationScheduleRepository.AddStudentsToScheduleAsync(scheduleId, studentIds, GetCurrentUserIdOrThrow());

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
                    var schedule = await _unitOfWork.VaccinationScheduleRepository.GetByIdAsync(scheduleId);
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

                    var result = await _unitOfWork.VaccinationScheduleRepository.RemoveStudentsFromScheduleAsync(scheduleId, studentIds);

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
                    var schedule = await _unitOfWork.VaccinationScheduleRepository.GetByIdAsync(scheduleId);
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

                    var result = await _unitOfWork.VaccinationScheduleRepository.UpdateScheduleStatusAsync(scheduleId, newStatus, GetCurrentUserIdOrThrow());

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
                    var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByIdsAsync(request.ScheduleIds);
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

                            var updateResult = await _unitOfWork.VaccinationScheduleRepository.UpdateScheduleStatusAsync(scheduleId, request.NewStatus, GetCurrentUserIdOrThrow());

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
                    var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByIdsAsync(ids, includeDeleted: isPermanent);
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
                                var canDelete = await _unitOfWork.VaccinationScheduleRepository.CanDeleteScheduleAsync(id);
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

                                await _unitOfWork.VaccinationScheduleRepository.DeleteAsync(id);
                            }
                            else
                            {
                                // Use BaseService DeleteAsync for soft delete
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
                    var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByIdsAsync(ids, includeDeleted: true);
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

                            var restoreResult = await _unitOfWork.VaccinationScheduleRepository.RestoreScheduleAsync(id, GetCurrentUserIdOrThrow());

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
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSoftDeletedSchedulesAsync(
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
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByStatusAsync(ScheduleStatus.Pending);
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
                var schedules = await _unitOfWork.VaccinationScheduleRepository.GetSchedulesByStatusAsync(ScheduleStatus.InProgress);
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

        private async Task<HashSet<Guid>> GetStudentIdsFromRequest(CreateVaccinationScheduleRequest request)
        {
            var studentIds = new HashSet<Guid>();

            // Add specific student IDs
            foreach (var studentId in request.StudentIds)
            {
                studentIds.Add(studentId);
            }

            // Add students by grade/section
            if (request.Grades.Any() || request.Sections.Any())
            {
                var students = await _unitOfWork.StudentRepository.GetStudentsByGradeAndSectionAsync(
                    request.Grades, request.Sections);

                foreach (var student in students)
                {
                    studentIds.Add(student.Id);
                }
            }

            // Add all students in specific grades if requested
            if (request.IncludeAllStudentsInGrades && request.Grades.Any())
            {
                var allStudentsInGrades = await _unitOfWork.StudentRepository.GetStudentsByGradeAndSectionAsync(
                    request.Grades, new List<string>());

                foreach (var student in allStudentsInGrades)
                {
                    studentIds.Add(student.Id);
                }
            }

            return studentIds;
        }

        private async Task<List<VaccinationSchedule>> CreateSchedulesFromStudentIds(
            CreateVaccinationScheduleRequest request, HashSet<Guid> studentIds)
        {
            var schedules = new List<VaccinationSchedule>();
            var baseScheduledTime = request.ScheduledAt;

            // Lọc ra những học sinh không có xung đột lịch tiêm
            var validStudentIds = new List<Guid>();

            foreach (var studentId in studentIds)
            {
                var hasStudentConflict = await _unitOfWork.VaccinationScheduleRepository
                    .HasStudentScheduleConflictAsync(studentId, baseScheduledTime);

                if (hasStudentConflict)
                {
                    _logger.LogWarning("Bỏ qua học sinh {StudentId} do xung đột lịch tiêm", studentId);
                    continue;
                }

                validStudentIds.Add(studentId);
            }

            // Nếu có học sinh hợp lệ, tạo MỘT schedule duy nhất
            if (validStudentIds.Any())
            {
                var sessionStudents = validStudentIds.Select(studentId => new SessionStudent
                {
                    // Don't set Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy - BaseService will handle these
                    StudentId = studentId,
                    ConsentStatus = ParentConsentStatus.Pending,
                    ConsentDeadline = baseScheduledTime.AddDays(-2), // 2 days before vaccination
                    IsDeleted = false
                }).ToList();

                var schedule = new VaccinationSchedule
                {
                    // Don't set Id, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy - BaseService will handle these
                    CampaignId = request.CampaignId,
                    VaccinationTypeId = request.VaccinationTypeId,
                    ScheduledAt = baseScheduledTime,
                    ScheduleStatus = ScheduleStatus.Pending,
                    IsDeleted = false,
                    SessionStudents = sessionStudents // Gán tất cả SessionStudent vào một schedule
                };

                schedules.Add(schedule);

                _logger.LogInformation("Tạo 1 lịch tiêm với {Count} học sinh cho thời gian {ScheduledTime}",
                    validStudentIds.Count, baseScheduledTime);
            }

            return schedules;
        }

        private static bool HasValidSelection(CreateVaccinationScheduleRequest request)
        {
            return (request.StudentIds != null && request.StudentIds.Any())
                || (request.Grades != null && request.Grades.Any())
                || (request.Sections != null && request.Sections.Any())
                || request.IncludeAllStudentsInGrades;
        }

        #endregion
    }
}