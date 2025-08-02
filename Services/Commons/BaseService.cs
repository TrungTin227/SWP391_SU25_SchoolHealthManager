namespace Services.Commons
{
    public abstract class BaseService<TEntity, TKey>
    where TEntity : class
    {
        protected readonly IGenericRepository<TEntity, TKey> _repository;
        protected readonly ICurrentUserService _currentUserService;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ICurrentTime _currentTime;

        protected BaseService(
            IGenericRepository<TEntity, TKey> repository,
            ICurrentUserService currentUserService,
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _currentTime = currentTime;
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            // Service handle audit fields for main entity and related entities
            SetAuditFieldsForCreate(entity);
            SetAuditFieldsForNavigationProperties(entity, isCreate: true);

            var result = await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            // Service handle audit fields for main entity and related entities
            SetAuditFieldsForUpdate(entity);
            SetAuditFieldsForNavigationProperties(entity, isCreate: false);

            var result = await _repository.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var currentUserId = _currentUserService.GetUserId();
            var result = await _repository.SoftDeleteAsync(id, currentUserId);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        private void SetAuditFieldsForCreate(TEntity entity)
        {
            var currentUserId = _currentUserService.GetUserId();
            var now = _currentTime.GetVietnamTime();

            if (IsInheritedFromBaseEntity(entity.GetType()))
            {
                SetProperty(entity, "CreatedAt", now);
                SetProperty(entity, "UpdatedAt", now);
                SetProperty(entity, "CreatedBy", currentUserId);
                SetProperty(entity, "UpdatedBy", currentUserId);

                // Auto-generate Id nếu là Guid và empty
                if (typeof(TKey) == typeof(Guid))
                {
                    var id = GetProperty<TKey>(entity, "Id");
                    if (id is Guid guidId && guidId == Guid.Empty)
                    {
                        SetProperty(entity, "Id", Guid.NewGuid());
                    }
                }
            }
            else if (entity is IBaseEntity<TKey> auditableEntity)
            {
                auditableEntity.CreatedAt = now;
                auditableEntity.UpdatedAt = now;
                auditableEntity.CreatedBy = currentUserId;
                auditableEntity.UpdatedBy = currentUserId;

                if (auditableEntity.Id is Guid guidId && guidId == Guid.Empty)
                {
                    auditableEntity.Id = (TKey)(object)Guid.NewGuid();
                }
            }
        }

        private void SetAuditFieldsForUpdate(TEntity entity)
        {
            var currentUserId = _currentUserService.GetUserId();
            var now = _currentTime.GetVietnamTime();

            if (IsInheritedFromBaseEntity(entity.GetType()))
            {
                SetProperty(entity, "UpdatedAt", now);
                SetProperty(entity, "UpdatedBy", currentUserId);
            }
            else if (entity is IBaseEntity<TKey> auditableEntity)
            {
                auditableEntity.UpdatedAt = now;
                auditableEntity.UpdatedBy = currentUserId;
            }
        }

        // Xử lý audit fields cho navigation properties
        private void SetAuditFieldsForNavigationProperties(TEntity entity, bool isCreate)
        {
            var currentUserId = _currentUserService.GetUserId() ?? Guid.Empty;
            var now = _currentTime.GetVietnamTime();

            var properties = entity.GetType().GetProperties();

            foreach (var property in properties)
            {
                // Handle collection navigation properties
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
                    && property.PropertyType.IsGenericType
                    && property.PropertyType != typeof(string))
                {
                    var collection = property.GetValue(entity) as System.Collections.IEnumerable;
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            SetAuditFieldsForRelatedEntity(item, currentUserId, now, isCreate);
                        }
                    }
                }
                // Handle single navigation properties
                else if (property.PropertyType.IsClass
                        && property.PropertyType != typeof(string)
                        && !property.PropertyType.IsPrimitive)
                {
                    var relatedEntity = property.GetValue(entity);
                    if (relatedEntity != null)
                    {
                        SetAuditFieldsForRelatedEntity(relatedEntity, currentUserId, now, isCreate);
                    }
                }
            }
        }

        private void SetAuditFieldsForRelatedEntity(object relatedEntity, Guid currentUserId, DateTime now, bool isCreate)
        {
            if (IsInheritedFromBaseEntity(relatedEntity.GetType()))
            {
                if (isCreate)
                {
                    SetProperty(relatedEntity, "CreatedAt", now);
                    SetProperty(relatedEntity, "UpdatedAt", now);
                    SetProperty(relatedEntity, "CreatedBy", currentUserId);
                    SetProperty(relatedEntity, "UpdatedBy", currentUserId);

                    // Auto-generate Id nếu là Guid và empty
                    var idProperty = relatedEntity.GetType().GetProperty("Id");
                    if (idProperty?.PropertyType == typeof(Guid))
                    {
                        var id = (Guid)idProperty.GetValue(relatedEntity);
                        if (id == Guid.Empty)
                        {
                            SetProperty(relatedEntity, "Id", Guid.NewGuid());
                        }
                    }
                }
                else
                {
                    SetProperty(relatedEntity, "UpdatedAt", now);
                    SetProperty(relatedEntity, "UpdatedBy", currentUserId);
                }
            }
        }
        public bool IsWithinWorkingHours(DateTime time)
        {
            var hour = time.Hour;
            return hour >= 8 && hour < 18;
        }
        public bool IsWithinWorkingHours(TimeSpan time)
        {
            return time >= TimeSpan.FromHours(7) && time <= TimeSpan.FromHours(22);
        }

        // Helper methods
        private bool IsInheritedFromBaseEntity(Type type)
        {
            return typeof(BaseEntity).IsAssignableFrom(type);
        }

        private void SetProperty(object obj, string propertyName, object value)
        {
            var property = obj.GetType().GetProperty(propertyName);
            property?.SetValue(obj, value);
        }

        private T GetProperty<T>(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property != null ? (T)property.GetValue(obj) : default;
        }
    }
}