using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Commons;
using System.Data;

namespace Services.Implementations
{
    public class HealthEventService : BaseService<HealthEvent, Guid>, IHealthEventService
    {
        private readonly ILogger<HealthEventService> _logger;

        public HealthEventService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ILogger<HealthEventService> logger)
            : base(unitOfWork.HealthEventRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null, bool filterByCurrentUser = false)
        {
            try
            {
                // Lấy current user ID
                Guid? currentUserId = null;
                if (filterByCurrentUser)
                {
                    currentUserId = _currentUserService.GetUserId();
                    if (!currentUserId.HasValue)
                    {
                        return ApiResult<PagedList<HealthEventResponseDTO>>.Failure(
                            new UnauthorizedAccessException("Không thể xác định người dùng hiện tại"));
                    }
                }
                var events = await _unitOfWork.HealthEventRepository.GetHealthEventsAsync(
                    pageNumber, pageSize, searchTerm, status, eventType, studentId, fromDate, toDate);

                var result = events.ToPagedResponseDTO();

                return ApiResult<PagedList<HealthEventResponseDTO>>.Success(
                    result, "Lấy danh sách sự kiện y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health events");
                return ApiResult<PagedList<HealthEventResponseDTO>>.Failure(ex);
            }
        }

        public async Task<ApiResult<HealthEventDetailResponseDTO>> GetHealthEventByIdAsync(Guid id)
        {
            try
            {
                var healthEvent = await _unitOfWork.HealthEventRepository.GetHealthEventWithDetailsAsync(id);
                if (healthEvent == null)
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(
                        new Exception("Không tìm thấy sự kiện y tế"));

                return ApiResult<HealthEventDetailResponseDTO>.Success(
                    HealthEventMapper.MapToDetailResponseDTO(healthEvent),
                    "Lấy thông tin chi tiết sự kiện y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health event by ID: {EventId}", id);
                return ApiResult<HealthEventDetailResponseDTO>.Failure(ex);
            }
        }

        public async Task<ApiResult<HealthEventResponseDTO>> CreateHealthEventAsync(CreateHealthEventRequestDTO request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {

                    var validationResult = await ValidateCreateRequestAsync(request);
                    if (!validationResult.IsSuccess)
                    {
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception(validationResult.Message));
                    }

                    var healthEvent = HealthEventMapper.MapFromCreateRequest(request);
                    var today = _currentTime.GetVietnamTime().Date;
                    var todayCount = await _unitOfWork.HealthEventRepository
                                                      .GetQueryable()
                                                      .CountAsync(e => e.CreatedAt >= today);
                    healthEvent.EventCode = $"EV-{today:yyyyMMdd}-{todayCount + 1:000}";

                    // Tự động set trạng thái là Pending
                    healthEvent.EventStatus = EventStatus.Pending;
                    healthEvent.ReportedUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    var createdEvent = await CreateAsync(healthEvent);

                    var eventDTO = HealthEventMapper.MapToResponseDTO(createdEvent);

                    _logger.LogInformation("Health event created with status Pending: {EventId}", createdEvent.Id);

                    return ApiResult<HealthEventResponseDTO>.Success(
                        eventDTO, "Tạo sự kiện y tế thành công với trạng thái Pending");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating health event");
                    return ApiResult<HealthEventResponseDTO>.Failure(ex);
                }
            });
        }
        #endregion

        #region Workflow Operations

        public async Task<ApiResult<HealthEventResponseDTO>> UpdateHealthEventWithTreatmentAsync(UpdateHealthEventRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var healthEvent = await _unitOfWork.HealthEventRepository.GetByIdAsync(request.HealthEventId);
                    if (healthEvent == null)
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Không tìm thấy sự kiện y tế"));

                    if (healthEvent.EventStatus == EventStatus.Resolved)
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Không thể cập nhật sự kiện y tế đã hoàn thành"));

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var vietnamTime = _currentTime.GetVietnamTime();
                    var hasAnyTreatment = false;

                    // Cập nhật thông tin chi tiết (tất cả đều optional)
                    if (request.Location != null) healthEvent.Location = request.Location;
                    if (request.InjuredBodyPartsRaw != null) healthEvent.InjuredBodyPartsRaw = request.InjuredBodyPartsRaw;
                    if (request.Severity != null) healthEvent.Severity = request.Severity;
                    if (request.Symptoms != null) healthEvent.Symptoms = request.Symptoms;
                    if (request.FirstAidAt != null) healthEvent.FirstAidAt = request.FirstAidAt;
                    if (request.FirstResponderId != null) healthEvent.FirstResponderId = request.FirstResponderId;
                    if (request.FirstAidDescription != null) healthEvent.FirstAidDescription = request.FirstAidDescription;
                    if (request.ParentNotifiedAt != null) healthEvent.ParentNotifiedAt = request.ParentNotifiedAt;
                    if (request.ParentNotificationMethod != null) healthEvent.ParentNotificationMethod = request.ParentNotificationMethod;
                    if (request.ParentNotificationNote != null) healthEvent.ParentNotificationNote = request.ParentNotificationNote;
                    if (request.IsReferredToHospital != null) healthEvent.IsReferredToHospital = request.IsReferredToHospital;
                    if (request.ReferralHospital != null) healthEvent.ReferralHospital = request.ReferralHospital;
                    if (request.ReferralDepartureTime != null) healthEvent.ReferralDepartureTime = request.ReferralDepartureTime;
                    if (request.ReferralTransportBy != null) healthEvent.ReferralTransportBy = request.ReferralTransportBy;
                    if (request.ParentSignatureUrl != null) healthEvent.ParentSignatureUrl = request.ParentSignatureUrl;
                    if (request.ParentArrivalAt != null) healthEvent.ParentArrivalAt = request.ParentArrivalAt;
                    if (request.ParentReceivedBy != null) healthEvent.ParentReceivedBy = request.ParentReceivedBy;
                    if (request.AdditionalNotes != null) healthEvent.AdditionalNotes = request.AdditionalNotes;
                    if (request.AttachmentUrlsRaw != null) healthEvent.AttachmentUrlsRaw = request.AttachmentUrlsRaw;
                    if (request.WitnessesRaw != null) healthEvent.WitnessesRaw = request.WitnessesRaw;

                    // Xử lý vật tư y tế
                    if (request.SupplyUsages != null && request.SupplyUsages.Any())
                    {
                        var supplyValidation = await ValidateSupplyUsagesAsync(request.SupplyUsages, currentUserId);
                        if (!supplyValidation.IsSuccess)
                            return ApiResult<HealthEventResponseDTO>.Failure(new Exception(supplyValidation.Message));

                        await ProcessSupplyUsagesAsync(request.HealthEventId, request.SupplyUsages, currentUserId, vietnamTime);
                        hasAnyTreatment = true;
                    }

                    // Chuyển trạng thái nếu có điều trị
                    if (hasAnyTreatment && healthEvent.EventStatus == EventStatus.Pending)
                    {
                        await _unitOfWork.HealthEventRepository.UpdateEventStatusAsync(
                            request.HealthEventId, EventStatus.InProgress, currentUserId);
                    }

                    await UpdateAsync(healthEvent);   // chứa UpdatedAt/UpdatedBy
                    await _unitOfWork.SaveChangesAsync();

                    var updatedEvent = await _unitOfWork.HealthEventRepository.GetByIdAsync(request.HealthEventId);
                    var eventDTO = HealthEventMapper.MapToResponseDTO(updatedEvent!);

                    var message = hasAnyTreatment && updatedEvent!.EventStatus == EventStatus.InProgress
                        ? "Cập nhật điều trị thành công – trạng thái chuyển sang InProgress"
                        : "Cập nhật điều trị sự kiện y tế thành công";

                    _logger.LogInformation("Health event {EventId} updated with treatment, new status: {Status}",
                        request.HealthEventId, updatedEvent.EventStatus);

                    return ApiResult<HealthEventResponseDTO>.Success(eventDTO, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating health event with treatment: {EventId}", request.HealthEventId);
                    return ApiResult<HealthEventResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<HealthEventResponseDTO>> ResolveHealthEventAsync(ResolveHealthEventRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var healthEvent = await _unitOfWork.HealthEventRepository.GetByIdAsync(request.HealthEventId);
                    if (healthEvent == null)
                    {
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Không tìm thấy sự kiện y tế"));
                    }

                    // Kiểm tra trạng thái có thể resolve
                    if (healthEvent.EventStatus != EventStatus.InProgress)
                    {
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Sự kiện y tế phải ở trạng thái InProgress để có thể hoàn thành"));
                    }

                    // Cập nhật trạng thái sang Resolved
                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    await _unitOfWork.HealthEventRepository.UpdateEventStatusAsync(request.HealthEventId, EventStatus.Resolved, currentUserId);

                    // Nếu có ghi chú hoàn thành, cập nhật vào description
                    if (!string.IsNullOrWhiteSpace(request.CompletionNotes))
                    {
                        healthEvent.Description += $"\n\nGhi chú hoàn thành: {request.CompletionNotes}";
                        await UpdateAsync(healthEvent);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Lấy lại dữ liệu để trả về
                    var updatedEvent = await _unitOfWork.HealthEventRepository.GetByIdAsync(request.HealthEventId);
                    var eventDTO = HealthEventMapper.MapToResponseDTO(updatedEvent!);

                    _logger.LogInformation("Health event {EventId} resolved successfully", request.HealthEventId);

                    return ApiResult<HealthEventResponseDTO>.Success(
                        eventDTO, "Hoàn thành xử lý sự kiện y tế thành công");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolving health event: {EventId}", request.HealthEventId);
                    return ApiResult<HealthEventResponseDTO>.Failure(ex);
                }
            });
        }

        #endregion

        #region Batch Operations

        public async Task<ApiResult<BatchOperationResultDTO>> DeleteHealthEventsAsync(List<Guid> ids, bool isPermanent = false)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(ids, isPermanent ? "xóa vĩnh viễn" : "xóa");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    var operationType = isPermanent ? "xóa vĩnh viễn" : "xóa";
                    _logger.LogInformation("Starting batch {Operation} for {Count} health events",
                        operationType, ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    if (isPermanent)
                    {
                        var allEvents = await _unitOfWork.HealthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: true);
                        var existingIds = allEvents.Select(e => e.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy sự kiện y tế");

                        if (existingIds.Any())
                        {
                            var deletedCount = await _unitOfWork.HealthEventRepository.PermanentDeleteHealthEventsAsync(existingIds);

                            if (deletedCount > 0)
                            {
                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }
                    else
                    {
                        var existingEvents = await _unitOfWork.HealthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: false);
                        var existingIds = existingEvents.Select(e => e.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy sự kiện y tế hoặc đã bị xóa");

                        if (existingIds.Any())
                        {
                            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                            var deletedCount = await _unitOfWork.HealthEventRepository.SoftDeleteHealthEventsAsync(existingIds, currentUserId);

                            if (deletedCount > 0)
                            {
                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage(operationType, result);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch delete of health events (isPermanent: {IsPermanent})", isPermanent);
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        public async Task<ApiResult<BatchOperationResultDTO>> RestoreHealthEventsAsync(List<Guid> ids)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var validationResult = ValidateBatchInput(ids, "khôi phục");
                    if (!validationResult.isValid)
                    {
                        return ApiResult<BatchOperationResultDTO>.Failure(
                            new ArgumentException(validationResult.message));
                    }

                    _logger.LogInformation("Starting batch restore for {Count} health events", ids.Count);

                    var result = new BatchOperationResultDTO
                    {
                        TotalRequested = ids.Count
                    };

                    var deletedEvents = await _unitOfWork.HealthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: true);
                    var deletedEventIds = deletedEvents.Where(e => e.IsDeleted).Select(e => e.Id).ToList();
                    var notDeletedIds = ids.Except(deletedEventIds).ToList();

                    foreach (var notDeletedId in notDeletedIds)
                    {
                        var healthEvent = deletedEvents.FirstOrDefault(e => e.Id == notDeletedId);
                        string errorMessage = healthEvent == null
                            ? "Không tìm thấy sự kiện y tế"
                            : "Sự kiện y tế chưa bị xóa";

                        result.Errors.Add(new BatchOperationErrorDTO
                        {
                            Id = notDeletedId.ToString(),
                            Error = errorMessage,
                            Details = $"Health event với ID {notDeletedId} {errorMessage.ToLower()}"
                        });
                    }

                    if (deletedEventIds.Any())
                    {
                        var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                        var restoredCount = await _unitOfWork.HealthEventRepository.RestoreHealthEventsAsync(deletedEventIds, currentUserId);

                        if (restoredCount > 0)
                        {
                            result.SuccessCount = restoredCount;
                            result.SuccessIds = deletedEventIds.Select(id => id.ToString()).ToList();
                        }
                    }

                    result.FailureCount = result.Errors.Count;
                    result.Message = GenerateBatchOperationMessage("khôi phục", result);

                    return ApiResult<BatchOperationResultDTO>.Success(result, result.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch restore of health events");
                    return ApiResult<BatchOperationResultDTO>.Failure(ex);
                }
            }, IsolationLevel.ReadCommitted);
        }

        #endregion

        #region Soft Delete Operations

        public async Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetSoftDeletedEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var events = await _unitOfWork.HealthEventRepository.GetSoftDeletedEventsAsync(
                    pageNumber, pageSize, searchTerm);

                var result = events.ToPagedResponseDTO();

                return ApiResult<PagedList<HealthEventResponseDTO>>.Success(
                    result, "Lấy danh sách sự kiện y tế đã xóa thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting soft deleted events");
                return ApiResult<PagedList<HealthEventResponseDTO>>.Failure(ex);
            }
        }

        #endregion

        #region Statistics

        public async Task<ApiResult<HealthEventStatisticsResponseDTO>> GetStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var statusStats = await _unitOfWork.HealthEventRepository.GetEventStatusStatisticsAsync(fromDate, toDate);
                var typeStats = await _unitOfWork.HealthEventRepository.GetEventTypeStatisticsAsync(fromDate, toDate);

                var statistics = new HealthEventStatisticsResponseDTO
                {
                    TotalEvents = statusStats.Values.Sum(),
                    PendingEvents = statusStats.GetValueOrDefault(EventStatus.Pending, 0),
                    InProgressEvents = statusStats.GetValueOrDefault(EventStatus.InProgress, 0),
                    ResolvedEvents = statusStats.GetValueOrDefault(EventStatus.Resolved, 0),
                    EventsByType = typeStats.ToDictionary(
                        kvp => kvp.Key.ToString(),
                        kvp => kvp.Value),
                    FromDate = fromDate,
                    ToDate = toDate
                };

                return ApiResult<HealthEventStatisticsResponseDTO>.Success(
                    statistics, "Lấy thống kê sự kiện y tế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health event statistics");
                return ApiResult<HealthEventStatisticsResponseDTO>.Failure(ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<(bool IsSuccess, string Message)> ValidateCreateRequestAsync(CreateHealthEventRequestDTO request)
        {
            // Validate student exists
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                return (false, "Không tìm thấy học sinh");
            }

            // Validate vaccination record if EventCategory is Vaccination
            if (request.EventCategory == EventCategory.Vaccination)
            {
                if (!request.VaccinationRecordId.HasValue)
                {
                    return (false, "Bản ghi tiêm chủng là bắt buộc khi phân loại sự kiện là Vaccination");
                }

                var vaccinationRecord = await _unitOfWork.GetRepository<VaccinationRecord, Guid>()
                    .GetByIdAsync(request.VaccinationRecordId.Value);
                if (vaccinationRecord == null)
                {
                    return (false, "Không tìm thấy bản ghi tiêm chủng");
                }
            }
            else if (request.EventCategory == EventCategory.General && request.VaccinationRecordId.HasValue)
            {
                return (false, "Không nên có bản ghi tiêm chủng khi phân loại sự kiện là General");
            }

            // Validate occurred date
            if (request.OccurredAt > _currentTime.GetVietnamTime())
            {
                return (false, "Thời điểm xảy ra không được lớn hơn thời điểm hiện tại");
            }

            return (true, "Valid");
        }

        private async Task<(bool IsSuccess, string Message)> ValidateEventMedicationsAsync(List<CreateEventMedicationRequest> medications)
        {
            foreach (var medication in medications)
            {
                var medicationLot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(medication.MedicationLotId);
                if (medicationLot == null || medicationLot.IsDeleted)
                {
                    return (false, $"Không tìm thấy lô thuốc với ID: {medication.MedicationLotId}");
                }

                if (medicationLot.Quantity < medication.Quantity)
                {
                    return (false, $"Số lượng thuốc {medicationLot.Medication?.Name} không đủ. Còn lại: {medicationLot.Quantity}, yêu cầu: {medication.Quantity}");
                }

                if (medicationLot.ExpiryDate.Date <= DateTime.UtcNow.Date)
                {
                    return (false, $"Lô thuốc {medicationLot.LotNumber} đã hết hạn");
                }
            }

            return (true, "Valid");
        }

        private async Task<(bool IsSuccess, string Message)> ValidateSupplyUsagesAsync(List<CreateSupplyUsageRequest> supplies, Guid currentUserId)
        {
            // Kiểm tra current user có NurseProfile không
            var nurseProfiles = await _unitOfWork.GetRepository<NurseProfile, Guid>()
                .GetAllAsync(np => np.UserId == currentUserId);
            var nurseProfile = nurseProfiles.FirstOrDefault();

            if (nurseProfile == null)
            {
                return (false, "Người dùng hiện tại không phải là y tá hoặc chưa có hồ sơ y tá");
            }

            foreach (var supply in supplies)
            {
                var medicalSupplyLot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(supply.MedicalSupplyLotId);
                if (medicalSupplyLot == null || medicalSupplyLot.IsDeleted)
                {
                    return (false, $"Không tìm thấy lô vật tư y tế với ID: {supply.MedicalSupplyLotId}");
                }

                if (medicalSupplyLot.Quantity < supply.QuantityUsed)
                {
                    return (false, $"Số lượng vật tư {medicalSupplyLot.MedicalSupply?.Name} không đủ. Còn lại: {medicalSupplyLot.Quantity}, yêu cầu: {supply.QuantityUsed}");
                }

                if (medicalSupplyLot.ExpirationDate.Date <= DateTime.UtcNow.Date)
                {
                    return (false, $"Lô vật tư {medicalSupplyLot.LotNumber} đã hết hạn");
                }
            }

            return (true, "Valid");
        }

        private async Task ProcessEventMedicationsAsync(Guid healthEventId, List<CreateEventMedicationRequest> medications,
            Guid currentUserId, DateTime vietnamTime)
        {
            foreach (var medication in medications)
            {
                // Tạo EventMedication
                var eventMedication = new EventMedication
                {
                    HealthEventId = healthEventId,
                    MedicationLotId = medication.MedicationLotId,
                    Quantity = medication.Quantity,
                    CreatedAt = vietnamTime,
                    CreatedBy = currentUserId,
                    UpdatedAt = vietnamTime,
                    UpdatedBy = currentUserId
                };

                await _unitOfWork.GetRepository<EventMedication, Guid>().AddAsync(eventMedication);

                // Cập nhật số lượng thuốc
                var medicationLot = await _unitOfWork.MedicationLotRepository.GetByIdAsync(medication.MedicationLotId);
                if (medicationLot != null)
                {
                    medicationLot.Quantity -= medication.Quantity;
                    medicationLot.UpdatedAt = vietnamTime;
                    medicationLot.UpdatedBy = currentUserId;
                }
            }
        }

        private async Task ProcessSupplyUsagesAsync(Guid healthEventId, List<CreateSupplyUsageRequest> supplies,
            Guid currentUserId, DateTime vietnamTime)
        {
            // Lấy NurseProfile của current user
            var nurseProfiles = await _unitOfWork.GetRepository<NurseProfile, Guid>()
                .GetAllAsync(np => np.UserId == currentUserId);
            var nurseProfile = nurseProfiles.FirstOrDefault();

            if (nurseProfile == null)
            {
                throw new Exception("Không tìm thấy hồ sơ y tá của người dùng hiện tại");
            }

            foreach (var supply in supplies)
            {
                // Tạo SupplyUsage với NurseProfileId từ current user
                var supplyUsage = new SupplyUsage
                {
                    HealthEventId = healthEventId,
                    MedicalSupplyLotId = supply.MedicalSupplyLotId,
                    QuantityUsed = supply.QuantityUsed,
                    NurseProfileId = nurseProfile.UserId, // Lấy từ current user
                    Notes = supply.Notes ?? string.Empty,
                    CreatedAt = vietnamTime,
                    CreatedBy = currentUserId,
                    UpdatedAt = vietnamTime,
                    UpdatedBy = currentUserId
                };

                await _unitOfWork.GetRepository<SupplyUsage, Guid>().AddAsync(supplyUsage);

                // Cập nhật số lượng vật tư
                var medicalSupplyLot = await _unitOfWork.MedicalSupplyLotRepository.GetByIdAsync(supply.MedicalSupplyLotId);
                if (medicalSupplyLot != null)
                {
                    medicalSupplyLot.Quantity -= supply.QuantityUsed;
                    medicalSupplyLot.UpdatedAt = vietnamTime;
                    medicalSupplyLot.UpdatedBy = currentUserId;
                }
            }
        }

        private static (bool isValid, string message) ValidateBatchInput(List<Guid> ids, string operation)
        {
            if (ids == null || !ids.Any())
            {
                return (false, "Danh sách ID không được rỗng");
            }

            if (ids.Any(id => id == Guid.Empty))
            {
                return (false, "Danh sách chứa ID không hợp lệ");
            }

            return (true, string.Empty);
        }

        private static void AddErrorsForNotFoundItems(BatchOperationResultDTO result, List<Guid> notFoundIds, string errorMessage)
        {
            foreach (var notFoundId in notFoundIds)
            {
                result.Errors.Add(new BatchOperationErrorDTO
                {
                    Id = notFoundId.ToString(),
                    Error = errorMessage,
                    Details = $"Health event với ID {notFoundId} {errorMessage.ToLower()}"
                });
            }
        }

        private static string GenerateBatchOperationMessage(string operation, BatchOperationResultDTO result)
        {
            if (result.IsCompleteSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount} sự kiện y tế";
            }
            else if (result.IsCompleteFailure)
            {
                return $"Không thể {operation} bất kỳ sự kiện y tế nào. {result.FailureCount} lỗi";
            }
            else if (result.IsPartialSuccess)
            {
                return $"Đã {operation} thành công {result.SuccessCount}/{result.TotalRequested} sự kiện y tế. {result.FailureCount} lỗi";
            }
            else
            {
                return $"Không có sự kiện y tế nào được {operation}";
            }
        }

        public async Task<ApiResult<List<HealthEventResponseDTO>>> GetHealthForParentAsync()
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                if (!currentUserId.HasValue)
                {
                    return ApiResult<List<HealthEventResponseDTO>>.Failure(
                        new UnauthorizedAccessException("Không thể xác định người dùng hiện tại!! \nHãy thử đăng nhập lại và thử lại!!!"));
                }

                // 1. Lấy danh sách học sinh (ID)
                var students = await _unitOfWork.StudentRepository.GetStudentsByParentIdAsync(currentUserId.Value);
                var studentIds = students.Select(s => s.Id).ToHashSet();

                if (!studentIds.Any())
                    return ApiResult<List<HealthEventResponseDTO>>.Failure(new Exception("Không tìm thấy học sinh nào liên kết với phụ huynh hiện tại!!"));

                // 2. Lấy danh sách sự kiện y tế của các học sinh đó
                var healthEvents = await _unitOfWork.HealthEventRepository.GetHealthEventsByStudentIdsAsync(studentIds);

                if (healthEvents == null || !healthEvents.Any())
                {
                    return ApiResult<List<HealthEventResponseDTO>>.Success(new List<HealthEventResponseDTO>(),"Không có sự kiện y tế nào xảy ra với các con của bạn!!");
                }

                // 3. Chuẩn bị dữ liệu liên quan:
                var studentDict = students.ToDictionary(s => s.Id, s => s.FirstName+s.LastName);

                // Lấy thông tin người báo cáo
                var reporterIds = healthEvents.Select(e => e.ReportedUserId).Distinct().ToList();
                var reporters = await _unitOfWork.UserRepository.GetByIdsAsync(reporterIds);
                var reporterDict = reporters.ToDictionary(u => u.Id, u => u.FullName);

                // 4. Map thủ công
                var result = healthEvents.Select(e => new HealthEventResponseDTO
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = studentDict.GetValueOrDefault(e.StudentId, "Không rõ"),
                    EventCategory = e.EventCategory.ToString(),
                    VaccinationRecordId = e.VaccinationRecordId,
                    EventType = e.EventType.ToString(),
                    Description = e.Description,
                    OccurredAt = e.OccurredAt,
                    EventStatus = e.EventStatus.ToString(),
                    ReportedBy = e.ReportedUserId,
                    ReportedByName = reporterDict.GetValueOrDefault(e.ReportedUserId, "Không rõ"),
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    IsDeleted = e.IsDeleted,

                    EventCode = e.EventCode ?? string.Empty,
                    Location = e.Location,
                    Severity = e.Severity?.ToString(),
                    ResolvedAt = e.ResolvedAt,
                    TotalMedications = e.EventMedications?.Count ?? 0,
                    TotalSupplies = e.SupplyUsages?.Count ?? 0
                }).ToList();

                return ApiResult<List<HealthEventResponseDTO>>.Success(result,"Lấy danh sách sự kiện y tế của con bạn thành công!!!");
            }
            catch (Exception ex)
            {
                return ApiResult<List<HealthEventResponseDTO>>.Failure(new Exception("Có lỗi xảy ra khi lấy sự kiện y tế cho phụ huynh!! Lỗi: " + ex.Message));
            }
        }

        #endregion
    }
}