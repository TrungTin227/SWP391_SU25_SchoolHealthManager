using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;

namespace Services.Implementations
{
    public class HealthEventService : BaseService<HealthEvent, Guid>, IHealthEventService
    {
        private readonly ILogger<HealthEventService> _logger;
        private readonly ISchoolHealthEmailService _emailService;
        private readonly IConfiguration _configuration; 
        public HealthEventService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ICurrentTime currentTime,
            ISchoolHealthEmailService emailService,
            IConfiguration configuration,
            ILogger<HealthEventService> logger)
            : base(unitOfWork.HealthEventRepository, currentUserService, unitOfWork, currentTime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _configuration = configuration;

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
                    var validation = await ValidateCreateRequestAsync(request);
                    if (!validation.IsSuccess)
                        return ApiResult<HealthEventResponseDTO>.Failure(new Exception(validation.Message));

                    var healthEvent = HealthEventMapper.MapFromCreateRequest(request);
                    var today = _currentTime.GetVietnamTime().Date;
                    var todayCount = await _unitOfWork.HealthEventRepository
                                                      .GetQueryable()
                                                      .CountAsync(e => e.CreatedAt >= today);
                    healthEvent.EventCode = $"EV-{today:yyyyMMdd}-{todayCount + 1:000}";
                    healthEvent.ReportedUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    healthEvent.EventStatus = EventStatus.Pending;

                    var created = await CreateAsync(healthEvent);
                    await _unitOfWork.SaveChangesAsync();

                    /* ----- GỬI MAIL ----- */
                    var student = await _unitOfWork.GetRepository<Student, Guid>()
                                                   .GetQueryable()
                                                   .Include(s => s.Parent)
                                                        .ThenInclude(p => p.User)
                                                   .FirstOrDefaultAsync(s => s.Id == created.StudentId);

                    var parentEmail = student?.Parent?.User?.Email;
                    if (!string.IsNullOrWhiteSpace(parentEmail))
                    {
                        // 1. Lấy secret (tên đúng trong User-Secrets / appsettings)
                        var secret = _configuration["JwtSettings:Key"];
                        if (string.IsNullOrWhiteSpace(secret))
                            throw new InvalidOperationException("JWT key not configured");

                        // 2. Tạo token xác nhận
                        var payload = $"{created.Id}|{_currentTime.GetVietnamTime():yyyyMMdd}";
                        var hmac = new System.Security.Cryptography.HMACSHA256(
                                       Encoding.UTF8.GetBytes(secret));
                        var ackToken = Convert.ToBase64String(
                                           hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));

                        // 3. Gửi duy nhất 1 mail có link xác nhận
                        await _emailService.SendHealthEventAckMailAsync(
                            parentEmail,
                            student.FullName,
                            created.Description,
                            created.FirstAidDescription ?? "Chưa có sơ cứu",
                            created.Id,
                            ackToken);
                    }

                    var dto = HealthEventMapper.MapToResponseDTO(created);
                    return ApiResult<HealthEventResponseDTO>.Success(dto, "Tạo sự kiện y tế thành công");
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

        public async Task<ApiResult<HealthEventDetailResponseDTO>> UpdateHealthEventAsync(Guid id, UpdateHealthEventRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var healthEvent = await _unitOfWork.HealthEventRepository.GetHealthEventWithDetailsAsync(id);
                if (healthEvent == null)
                {
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(new Exception("Không tìm thấy sự kiện y tế."));
                }
                if (healthEvent.EventStatus == EventStatus.Resolved)
                {
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(new Exception("Không thể chỉnh sửa sự kiện đã hoàn thành."));
                }

                // Ánh xạ các giá trị từ request sang entity
                healthEvent.EventCategory = request.EventCategory;
                healthEvent.EventType = request.EventType;
                healthEvent.Description = request.Description;
                healthEvent.OccurredAt = request.OccurredAt;
                healthEvent.Location = request.Location;
                healthEvent.InjuredBodyPartsRaw = request.InjuredBodyPartsRaw;
                healthEvent.Severity = request.Severity;
                healthEvent.Symptoms = request.Symptoms;
                healthEvent.VaccinationRecordId = request.VaccinationRecordId;
                healthEvent.FirstAidAt = request.FirstAidAt;
                healthEvent.FirstResponderId = request.FirstResponderId;
                healthEvent.FirstAidDescription = request.FirstAidDescription;
                healthEvent.ParentNotifiedAt = request.ParentNotifiedAt;
                healthEvent.ParentNotificationMethod = request.ParentNotificationMethod;
                healthEvent.ParentNotificationNote = request.ParentNotificationNote;
                healthEvent.IsReferredToHospital = request.IsReferredToHospital;
                healthEvent.ReferralHospital = request.ReferralHospital;
                healthEvent.AdditionalNotes = request.AdditionalNotes;
                healthEvent.AttachmentUrlsRaw = request.AttachmentUrlsRaw;
                healthEvent.WitnessesRaw = request.WitnessesRaw;

                healthEvent.UpdatedAt = _currentTime.GetVietnamTime();
                healthEvent.UpdatedBy = _currentUserService.GetUserId() ?? Guid.Empty;

                await _unitOfWork.SaveChangesAsync();

                var dto = HealthEventMapper.MapToDetailResponseDTO(healthEvent);
                return ApiResult<HealthEventDetailResponseDTO>.Success(dto, "Cập nhật sự kiện y tế thành công.");
            });
        }

        public async Task<ApiResult<HealthEventDetailResponseDTO>> ResolveHealthEventAsync(Guid id, ResolveHealthEventRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    // SỬA LỖI 1: Sử dụng đúng Repository và phương thức lấy chi tiết
                    var healthEvent = await _unitOfWork.HealthEventRepository
                                                       .GetHealthEventWithDetailsAsync(id); // Dùng HealthEventRepository và GetHealthEventWithDetailsAsync

                    if (healthEvent == null)
                    {
                        return ApiResult<HealthEventDetailResponseDTO>.Failure(
                            new Exception("Không tìm thấy sự kiện y tế"));
                    }

                    // Kiểm tra trạng thái có thể resolve (logic này đã đúng)
                    if (healthEvent.EventStatus != EventStatus.InProgress)
                    {
                        return ApiResult<HealthEventDetailResponseDTO>.Failure(
                            new Exception("Sự kiện y tế phải ở trạng thái InProgress để có thể hoàn thành"));
                    }

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;

                    // Cập nhật các trường (logic này đã đúng)
                    healthEvent.EventStatus = EventStatus.Resolved;
                    healthEvent.ResolvedAt = _currentTime.GetVietnamTime();
                    healthEvent.UpdatedBy = currentUserId;
                    healthEvent.UpdatedAt = _currentTime.GetVietnamTime();

                    if (!string.IsNullOrWhiteSpace(request.CompletionNotes))
                    {
                        healthEvent.Description += $"\n\nGhi chú hoàn thành: {request.CompletionNotes}";
                    }

                    await _unitOfWork.SaveChangesAsync();

                    // Sử dụng Mapper chi tiết (logic này đã đúng)
                    var eventDTO = HealthEventMapper.MapToDetailResponseDTO(healthEvent);

                    // SỬA LỖI 2: Sử dụng tham số `id` từ phương thức cho việc ghi log
                    _logger.LogInformation("Health event {EventId} resolved successfully", id);

                    return ApiResult<HealthEventDetailResponseDTO>.Success(
                        eventDTO, "Hoàn thành xử lý sự kiện y tế thành công");
                }
                catch (Exception ex)
                {
                    // SỬA LỖI 2: Sử dụng tham số `id` từ phương thức cho việc ghi log
                    _logger.LogError(ex, "Error resolving health event: {EventId}", id);
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<HealthEventResponseDTO>> TreatHealthEventAsync(TreatHealthEventRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var he = await _unitOfWork.HealthEventRepository
                                              .GetHealthEventWithDetailsAsync(request.HealthEventId);
                    if (he == null)
                        return ApiResult<HealthEventResponseDTO>.Failure(new Exception("Không tìm thấy sự kiện y tế."));

                    if (he.EventStatus == EventStatus.Resolved)
                        return ApiResult<HealthEventResponseDTO>.Failure(new Exception("Đã hoàn tất, không thể chỉnh sửa."));

                    var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
                    var vnTime = _currentTime.GetVietnamTime();

                    /* ---- Cập nhật các trường ---- */
                    he.FirstAidAt = request.FirstAidAt;
                    he.FirstResponderId = request.FirstResponderId;
                    he.FirstAidDescription = request.FirstAidDescription;

                    if (request.Location is not null) he.Location = request.Location;
                    if (request.InjuredBodyPartsRaw is not null) he.InjuredBodyPartsRaw = request.InjuredBodyPartsRaw;
                    if (request.Severity is not null) he.Severity = request.Severity;
                    if (request.Symptoms is not null) he.Symptoms = request.Symptoms;

                    /* ---- Chuyển viện: chỉ gửi mail khi lần đầu đánh dấu ---- */
                    var oldReferral = he.IsReferredToHospital ?? false;
                    he.IsReferredToHospital = request.IsReferredToHospital ?? false;

                    if (he.IsReferredToHospital == true)
                    {
                        he.ReferralHospital = request.ReferralHospital;
                        he.ReferralDepartureTime = request.ReferralDepartureTime;
                        he.ReferralTransportBy = request.ReferralTransportBy;
                    }
                    else
                    {
                        he.ReferralHospital = null;
                        he.ReferralDepartureTime = null;
                        he.ReferralTransportBy = null;
                    }

                    /* ---- Thuốc ---- */
                    if (request.Medications is { Count: > 0 })
                    {
                        var medValid = await ValidateEventMedicationsAsync(request.Medications);
                        if (!medValid.IsSuccess)
                            return ApiResult<HealthEventResponseDTO>.Failure(new Exception(medValid.Message));
                        await ProcessEventMedicationsAsync(he.Id, request.Medications, currentUserId, vnTime);
                    }

                    /* ---- Vật tư ---- */
                    if (request.Supplies is { Count: > 0 })
                    {
                        var supValid = await ValidateSupplyUsagesAsync(request.Supplies, currentUserId);
                        if (!supValid.IsSuccess)
                            return ApiResult<HealthEventResponseDTO>.Failure(new Exception(supValid.Message));
                        await ProcessSupplyUsagesAsync(he.Id, request.Supplies, currentUserId, vnTime);
                    }

                    /* ---- Trạng thái ---- */
                    if (he.EventStatus == EventStatus.Pending)
                    {
                        he.EventStatus = EventStatus.InProgress;
                        await _unitOfWork.HealthEventRepository
                                         .UpdateEventStatusAsync(he.Id, EventStatus.InProgress, currentUserId);
                    }

                    he.UpdatedAt = vnTime;
                    he.UpdatedBy = currentUserId;
                    //await UpdateAsync(he);

                    await _unitOfWork.SaveChangesAsync();

                    /* ---- Gửi mail xác nhận chuyển viện nếu vừa đánh dấu ---- */
                    if (!oldReferral && he.IsReferredToHospital == true)
                    {
                        var parentEmail = he.Student?.Parent?.User?.Email;
                        if (!string.IsNullOrWhiteSpace(parentEmail))
                        {
                            var secret = _configuration["JwtSettings:Key"];
                            if (string.IsNullOrWhiteSpace(secret))
                                throw new InvalidOperationException("JWT key not configured");

                            var payload = $"{he.Id}|{vnTime:yyyyMMdd}";
                            var ackToken = Convert.ToBase64String(
                                new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret))
                                .ComputeHash(Encoding.UTF8.GetBytes(payload)));
                            var studentName = $"{he.Student?.FirstName} {he.Student?.LastName}".Trim();
                            if (string.IsNullOrWhiteSpace(studentName))
                                studentName = "Học sinh";
                            await _emailService.SendHospitalReferralAckAsync(
                                    parentEmail,
                                    $"{he.Student.FirstName} {he.Student.LastName}".Trim(),
                                    referralHospital: he.ReferralHospital ?? "Chưa xác định",
                                    departureTime: he.ReferralDepartureTime ?? DateTime.UtcNow,
                                    transportBy: he.ReferralTransportBy ?? "Chưa xác định",
                                    he.Id,
                                    ackToken);

                            // Cập nhật thời gian & phương thức tự động
                            he.ParentNotifiedAt = vnTime;
                            he.ParentNotificationMethod = "Email";
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }

                    var dto = HealthEventMapper.MapToResponseDTO(he);
                    return ApiResult<HealthEventResponseDTO>.Success(dto, "Cập nhật điều trị thành công.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi điều trị sự kiện {RequestId}", request.HealthEventId);
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

        // Thay đổi kiểu trả về để phản ánh đúng ý định
        public async Task<ApiResult<List<HealthEventDetailResponseDTO>>> GetHealthForParentAsync()
        {
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                if (!currentUserId.HasValue)
                {
                    // Trả về DTO chi tiết rỗng
                    return ApiResult<List<HealthEventDetailResponseDTO>>.Failure(
                        new UnauthorizedAccessException("Không thể xác định người dùng hiện tại!!"));
                }

                var students = await _unitOfWork.StudentRepository.GetStudentsByParentIdAsync(currentUserId.Value);
                var studentIds = students.Select(s => s.Id).ToHashSet();

                if (!studentIds.Any())
                    // Trả về DTO chi tiết rỗng
                    return ApiResult<List<HealthEventDetailResponseDTO>>.Success(new List<HealthEventDetailResponseDTO>(), "Không có sự kiện y tế nào liên quan đến con của bạn.");

                // Repository đã được sửa để có đầy đủ .Include()
                var healthEvents = await _unitOfWork.HealthEventRepository.GetHealthEventsByStudentIdsAsync(studentIds);

                if (healthEvents == null || !healthEvents.Any())
                {
                    // Trả về DTO chi tiết rỗng
                    return ApiResult<List<HealthEventDetailResponseDTO>>.Success(new List<HealthEventDetailResponseDTO>(), "Không có sự kiện y tế nào xảy ra với các con của bạn!!");
                }

                // SỬ DỤNG MAPPER CHI TIẾT
                var result = healthEvents.Select(he => HealthEventMapper.MapToDetailResponseDTO(he)).ToList();

                // Trả về kết quả với kiểu DTO chi tiết
                return ApiResult<List<HealthEventDetailResponseDTO>>.Success(result, "Lấy danh sách sự kiện y tế của con bạn thành công!!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sự kiện y tế cho phụ huynh có UserId: {UserId}", _currentUserService.GetUserId());
                // Trả về DTO chi tiết rỗng
                return ApiResult<List<HealthEventDetailResponseDTO>>.Failure(new Exception("Có lỗi xảy ra khi lấy sự kiện y tế cho phụ huynh."));
            }
        }

        #endregion
        public async Task<ApiResult<HealthEventDetailResponseDTO>> RecordParentHandoverAsync(Guid id, RecordParentHandoverRequest request)
        {
            return await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                try
                {
                    var healthEvent = await _unitOfWork.HealthEventRepository.GetHealthEventWithDetailsAsync(id);
                    if (healthEvent == null)
                    {
                        return ApiResult<HealthEventDetailResponseDTO>.Failure(new Exception("Không tìm thấy sự kiện y tế."));
                    }

                    // Lấy thông tin người dùng từ Claims, không cần gọi DB
                    var currentUserId = _currentUserService.GetUserId();
                    var currentUserFullName = _currentUserService.GetUserFullName();

                    // Kiểm tra thông tin người dùng
                    if (!currentUserId.HasValue || string.IsNullOrEmpty(currentUserFullName))
                    {
                        return ApiResult<HealthEventDetailResponseDTO>.Failure(new Exception("Không thể xác định thông tin người dùng thực hiện hành động."));
                    }

                    // Xử lý upload chữ ký (giả sử bạn có IFileStorageService)
                    string? finalSignatureUrl = request.ParentSignatureUrl;
                    if (request.ParentSignatureUrl != null && request.ParentSignatureUrl.StartsWith("data:image"))
                    {
                        // Giả định có dịch vụ IFileStorageService để xử lý upload
                        // finalSignatureUrl = await _fileStorageService.UploadImageFromBase64Async(request.ParentSignatureUrl, "signatures");
                    }

                    // Cập nhật các trường thông tin bàn giao
                    healthEvent.ParentArrivalAt = request.ParentArrivalAt;
                    healthEvent.ParentReceivedBy = currentUserFullName; // Lấy từ FullName của User
                    healthEvent.ParentSignatureUrl = finalSignatureUrl;

                    // Cập nhật thông tin audit
                    healthEvent.UpdatedAt = _currentTime.GetVietnamTime();
                    healthEvent.UpdatedBy = currentUserId.Value;

                    await _unitOfWork.SaveChangesAsync();

                    var dto = HealthEventMapper.MapToDetailResponseDTO(healthEvent);
                    return ApiResult<HealthEventDetailResponseDTO>.Success(dto, "Ghi nhận thông tin bàn giao cho phụ huynh thành công.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi ghi nhận bàn giao cho phụ huynh tại sự kiện {EventId}", id);
                    return ApiResult<HealthEventDetailResponseDTO>.Failure(ex);
                }
            });
        }

        public async Task<ApiResult<bool>> RecordParentAckAsync(Guid eventId)
        {
            try
            {
                var evt = await _unitOfWork.HealthEventRepository.GetByIdAsync(eventId);

                if (evt == null)
                    return ApiResult<bool>.Failure(new Exception($"Không tìm thấy sự kiện y tế với ID: {eventId}"));

                // Đã xác nhận rồi → skip
                if (evt.ParentAckStatus == ParentAcknowledgmentStatus.Acknowledged)
                    return ApiResult<bool>.Success(true, "Sự kiện đã được xác nhận trước đó.");

                // Cập nhật trạng thái và thời gian
                evt.ParentAckStatus = ParentAcknowledgmentStatus.Acknowledged;
                evt.UpdatedAt = _currentTime.GetVietnamTime();

                await _unitOfWork.SaveChangesAsync();

                return ApiResult<bool>.Success(true, "Xác nhận của phụ huynh đã được ghi nhận.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi nhận xác nhận của phụ huynh cho sự kiện {EventId}", eventId);
                return ApiResult<bool>.Failure(ex);
            }
        }
    }
}