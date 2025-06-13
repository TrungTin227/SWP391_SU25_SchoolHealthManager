using DTOs.HealthEventDTOs.Request;
using DTOs.HealthEventDTOs.Response;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Commons;
using System.Data;

namespace Services.Implementations
{
    public class HealthEventService : BaseService<HealthEvent, Guid>, IHealthEventService
    {
        private readonly IHealthEventRepository _healthEventRepository;
        private readonly IMedicationLotRepository _medicationLotRepository;
        private readonly IMedicalSupplyLotRepository _medicalSupplyLotRepository;
        private readonly ILogger<HealthEventService> _logger;
        private readonly ICurrentTime _currentTime;

        public HealthEventService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<HealthEventService> logger,
            ICurrentTime currentTime)
            : base(unitOfWork.HealthEventRepository, currentUserService, unitOfWork, currentTime)
        {
            _healthEventRepository = unitOfWork.HealthEventRepository;
            _medicationLotRepository = unitOfWork.MedicationLotRepository;
            _medicalSupplyLotRepository = unitOfWork.MedicalSupplyLotRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentTime = currentTime ?? throw new ArgumentNullException(nameof(currentTime));
        }

        #region Basic CRUD Operations

        public async Task<ApiResult<PagedList<HealthEventResponseDTO>>> GetHealthEventsAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            EventStatus? status = null, EventType? eventType = null,
            Guid? studentId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var events = await _healthEventRepository.GetHealthEventsAsync(
                    pageNumber, pageSize, searchTerm, status, eventType, studentId, fromDate, toDate);

                var eventDTOs = events.Select(MapToResponseDTO).ToList();
                var result = CreatePagedResult(events, eventDTOs);

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
                var healthEvent = await _healthEventRepository.GetHealthEventWithDetailsAsync(id);
                if (healthEvent == null)
                {
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(
                        new Exception("Không tìm thấy sự kiện y tế"));
                }

                var detailDTO = MapToDetailResponseDTO(healthEvent);
                return ApiResult<HealthEventDetailResponseDTO>.Success(
                    detailDTO, "Lấy thông tin chi tiết sự kiện y tế thành công");
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

                    var healthEvent = MapFromCreateRequest(request);

                    // Tự động set trạng thái là Pending
                    healthEvent.EventStatus = EventStatus.Pending;
                    healthEvent.ReportedUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    var createdEvent = await CreateAsync(healthEvent);

                    var eventDTO = MapToResponseDTO(createdEvent);

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
                    var healthEvent = await _healthEventRepository.GetByIdAsync(request.HealthEventId);
                    if (healthEvent == null)
                    {
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Không tìm thấy sự kiện y tế"));
                    }

                    // Kiểm tra trạng thái có thể cập nhật
                    if (healthEvent.EventStatus == EventStatus.Resolved)
                    {
                        return ApiResult<HealthEventResponseDTO>.Failure(
                            new Exception("Không thể cập nhật sự kiện y tế đã hoàn thành"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var vietnamTime = _currentTime.GetVietnamTime();
                    var hasAnyTreatment = false;

                    // Xử lý thuốc
                    if (request.EventMedications != null && request.EventMedications.Any())
                    {
                        var medicationValidation = await ValidateEventMedicationsAsync(request.EventMedications);
                        if (!medicationValidation.IsSuccess)
                        {
                            return ApiResult<HealthEventResponseDTO>.Failure(
                                new Exception(medicationValidation.Message));
                        }

                        await ProcessEventMedicationsAsync(request.HealthEventId, request.EventMedications, currentUserId, vietnamTime);
                        hasAnyTreatment = true;
                    }

                    // Xử lý vật tư y tế
                    if (request.SupplyUsages != null && request.SupplyUsages.Any())
                    {
                        var supplyValidation = await ValidateSupplyUsagesAsync(request.SupplyUsages, currentUserId);
                        if (!supplyValidation.IsSuccess)
                        {
                            return ApiResult<HealthEventResponseDTO>.Failure(
                                new Exception(supplyValidation.Message));
                        }

                        await ProcessSupplyUsagesAsync(request.HealthEventId, request.SupplyUsages, currentUserId, vietnamTime);
                        hasAnyTreatment = true;
                    }

                    // Cập nhật trạng thái dựa trên logic business
                    if (hasAnyTreatment && healthEvent.EventStatus == EventStatus.Pending)
                    {
                        // Có điều trị và đang Pending -> chuyển sang InProgress
                        await _healthEventRepository.UpdateEventStatusAsync(request.HealthEventId, EventStatus.InProgress, currentUserId);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Lấy lại dữ liệu để trả về
                    var updatedEvent = await _healthEventRepository.GetByIdAsync(request.HealthEventId);
                    var eventDTO = MapToResponseDTO(updatedEvent!);

                    var statusMessage = hasAnyTreatment && updatedEvent!.EventStatus == EventStatus.InProgress
                        ? "Thêm điều trị thành công và trạng thái được cập nhật thành InProgress"
                        : "Cập nhật điều trị sự kiện y tế thành công";

                    _logger.LogInformation("Health event {EventId} updated with treatment, new status: {Status}",
                        request.HealthEventId, updatedEvent.EventStatus);

                    return ApiResult<HealthEventResponseDTO>.Success(eventDTO, statusMessage);
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
                    var healthEvent = await _healthEventRepository.GetByIdAsync(request.HealthEventId);
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
                    await _healthEventRepository.UpdateEventStatusAsync(request.HealthEventId, EventStatus.Resolved, currentUserId);

                    // Nếu có ghi chú hoàn thành, cập nhật vào description
                    if (!string.IsNullOrWhiteSpace(request.CompletionNotes))
                    {
                        healthEvent.Description += $"\n\nGhi chú hoàn thành: {request.CompletionNotes}";
                        await UpdateAsync(healthEvent);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Lấy lại dữ liệu để trả về
                    var updatedEvent = await _healthEventRepository.GetByIdAsync(request.HealthEventId);
                    var eventDTO = MapToResponseDTO(updatedEvent!);

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
                        var allEvents = await _healthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: true);
                        var existingIds = allEvents.Select(e => e.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy sự kiện y tế");

                        if (existingIds.Any())
                        {
                            var deletedCount = await _healthEventRepository.PermanentDeleteHealthEventsAsync(existingIds);

                            if (deletedCount > 0)
                            {
                                result.SuccessCount = deletedCount;
                                result.SuccessIds = existingIds.Select(id => id.ToString()).ToList();
                            }
                        }
                    }
                    else
                    {
                        var existingEvents = await _healthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: false);
                        var existingIds = existingEvents.Select(e => e.Id).ToList();
                        var notFoundIds = ids.Except(existingIds).ToList();

                        AddErrorsForNotFoundItems(result, notFoundIds, "Không tìm thấy sự kiện y tế hoặc đã bị xóa");

                        if (existingIds.Any())
                        {
                            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                            var deletedCount = await _healthEventRepository.SoftDeleteHealthEventsAsync(existingIds, currentUserId);

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

                    var deletedEvents = await _healthEventRepository.GetHealthEventsByIdsAsync(ids, includeDeleted: true);
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
                        var restoredCount = await _healthEventRepository.RestoreHealthEventsAsync(deletedEventIds, currentUserId);

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
                var events = await _healthEventRepository.GetSoftDeletedEventsAsync(
                    pageNumber, pageSize, searchTerm);

                var eventDTOs = events.Select(MapToResponseDTO).ToList();
                var result = CreatePagedResult(events, eventDTOs);

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
                var statusStats = await _healthEventRepository.GetEventStatusStatisticsAsync(fromDate, toDate);
                var typeStats = await _healthEventRepository.GetEventTypeStatisticsAsync(fromDate, toDate);

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
                var medicationLot = await _medicationLotRepository.GetByIdAsync(medication.MedicationLotId);
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
            // You'll need a proper repository or method
            var nurseProfiles = await _unitOfWork.GetRepository<NurseProfile, Guid>()
                .GetAllAsync(np => np.UserId == currentUserId);
            var nurseProfile = nurseProfiles.FirstOrDefault();

            if (nurseProfile == null)
            {
                return (false, "Người dùng hiện tại không phải là y tá hoặc chưa có hồ sơ y tá");
            }

            foreach (var supply in supplies)
            {
                var medicalSupplyLot = await _medicalSupplyLotRepository.GetByIdAsync(supply.MedicalSupplyLotId);
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
                var medicationLot = await _medicationLotRepository.GetByIdAsync(medication.MedicationLotId);
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
                var medicalSupplyLot = await _medicalSupplyLotRepository.GetByIdAsync(supply.MedicalSupplyLotId);
                if (medicalSupplyLot != null)
                {
                    medicalSupplyLot.Quantity -= supply.QuantityUsed;
                    medicalSupplyLot.UpdatedAt = vietnamTime;
                    medicalSupplyLot.UpdatedBy = currentUserId;
                }
            }
        }

        private async Task UpdateEventStatusBasedOnTreatmentAsync(Guid eventId, bool hasAnyTreatment, bool? isResolved, Guid currentUserId)
        {
            var healthEvent = await _healthEventRepository.GetByIdAsync(eventId);
            if (healthEvent == null) return;

            // Logic cập nhật trạng thái tự động
            if (isResolved == true && healthEvent.EventStatus == EventStatus.InProgress)
            {
                // Chuyển sang Resolved
                await _healthEventRepository.UpdateEventStatusAsync(eventId, EventStatus.Resolved, currentUserId);
            }
            else if (hasAnyTreatment && healthEvent.EventStatus == EventStatus.Pending)
            {
                // Có điều trị và đang Pending -> chuyển sang InProgress
                await _healthEventRepository.UpdateEventStatusAsync(eventId, EventStatus.InProgress, currentUserId);
            }
        }

        private static string GetStatusUpdateMessage(EventStatus currentStatus, bool hasAnyTreatment, bool? isResolved)
        {
            return currentStatus switch
            {
                EventStatus.Pending => "Cập nhật sự kiện y tế thành công (trạng thái: Pending)",
                EventStatus.InProgress when hasAnyTreatment => "Thêm điều trị thành công và trạng thái được cập nhật thành InProgress",
                EventStatus.InProgress => "Cập nhật sự kiện y tế thành công (trạng thái: InProgress)",
                EventStatus.Resolved when isResolved == true => "Hoàn thành xử lý sự kiện y tế thành công (trạng thái: Resolved)",
                EventStatus.Resolved => "Cập nhật sự kiện y tế thành công (trạng thái: Resolved)",
                _ => "Cập nhật sự kiện y tế thành công"
            };
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

        private static HealthEvent MapFromCreateRequest(CreateHealthEventRequestDTO request)
        {
            return new HealthEvent
            {
                StudentId = request.StudentId,
                EventCategory = request.EventCategory,
                VaccinationRecordId = request.VaccinationRecordId,
                EventType = request.EventType,
                Description = request.Description,
                OccurredAt = request.OccurredAt
            };
        }
        private static HealthEventResponseDTO MapToResponseDTO(HealthEvent healthEvent)
        {
            return new HealthEventResponseDTO
            {
                Id = healthEvent.Id,
                StudentId = healthEvent.StudentId,
                StudentName = healthEvent.Student?.FullName ?? string.Empty,
                EventCategory = healthEvent.EventCategory.ToString(),
                VaccinationRecordId = healthEvent.VaccinationRecordId,
                EventType = healthEvent.EventType.ToString(),
                Description = healthEvent.Description,
                OccurredAt = healthEvent.OccurredAt,
                EventStatus = healthEvent.EventStatus.ToString(),
                ReportedBy = healthEvent.ReportedUserId,
                ReportedByName = healthEvent.ReportedUser?.FullName ?? string.Empty,
                CreatedAt = healthEvent.CreatedAt,
                UpdatedAt = healthEvent.UpdatedAt,
                IsDeleted = healthEvent.IsDeleted,
                TotalMedications = healthEvent.EventMedications?.Count ?? 0,
                TotalSupplies = healthEvent.SupplyUsages?.Count ?? 0
            };
        }

        private static HealthEventDetailResponseDTO MapToDetailResponseDTO(HealthEvent healthEvent)
        {
            var baseDto = MapToResponseDTO(healthEvent);

            return new HealthEventDetailResponseDTO
            {
                Id = baseDto.Id,
                StudentId = baseDto.StudentId,
                StudentName = baseDto.StudentName,
                EventCategory = baseDto.EventCategory,
                VaccinationRecordId = baseDto.VaccinationRecordId,
                EventType = baseDto.EventType,
                Description = baseDto.Description,
                OccurredAt = baseDto.OccurredAt,
                EventStatus = baseDto.EventStatus,
                ReportedBy = baseDto.ReportedBy,
                ReportedByName = baseDto.ReportedByName,
                CreatedAt = baseDto.CreatedAt,
                UpdatedAt = baseDto.UpdatedAt,
                IsDeleted = baseDto.IsDeleted,
                TotalMedications = baseDto.TotalMedications,
                TotalSupplies = baseDto.TotalSupplies,

                Medications = healthEvent.EventMedications?.Select(em => new EventMedicationResponseDTO
                {
                    Id = em.Id,
                    MedicationLotId = em.MedicationLotId,
                    MedicationName = em.MedicationLot?.Medication?.Name ?? string.Empty,
                    LotNumber = em.MedicationLot?.LotNumber ?? string.Empty,
                    Quantity = em.Quantity,
                    UsedAt = em.CreatedAt
                }).ToList() ?? new List<EventMedicationResponseDTO>(),

                Supplies = healthEvent.SupplyUsages?.Select(su => new SupplyUsageResponseDTO
                {
                    Id = su.Id,
                    HealthEventId = su.HealthEventId,
                    MedicalSupplyLotId = su.MedicalSupplyLotId,
                    MedicalSupplyName = su.MedicalSupplyLot?.MedicalSupply?.Name ?? string.Empty,
                    LotNumber = su.MedicalSupplyLot?.LotNumber ?? string.Empty,
                    QuantityUsed = su.QuantityUsed,
                    NurseProfileId = su.NurseProfileId,
                    NurseName = su.UsedByNurse?.User?.FullName ?? string.Empty,
                    Notes = su.Notes,
                    CreatedAt = su.CreatedAt
                }).ToList() ?? new List<SupplyUsageResponseDTO>()
            };
        }

        private static PagedList<HealthEventResponseDTO> CreatePagedResult(
            PagedList<HealthEvent> sourcePaged,
            List<HealthEventResponseDTO> mappedItems)
        {
            return new PagedList<HealthEventResponseDTO>(
                mappedItems,
                sourcePaged.MetaData.TotalCount,
                sourcePaged.MetaData.CurrentPage,
                sourcePaged.MetaData.PageSize);
        }

        #endregion
    }
}