namespace Repositories.Interfaces
{
    public interface IMedicationRepository : IGenericRepository<Medication, Guid>
    {
        Task<PagedList<Medication>> GetMedicationsAsync(int pageNumber, int pageSize, string? searchTerm = null, MedicationCategory? category = null);
        Task<bool> MedicationNameExistsAsync(string name, Guid? excludeId = null);
        Task<List<Medication>> GetMedicationsByCategoryAsync(MedicationCategory category);
        Task<List<Medication>> GetActiveMedicationsAsync();
        Task<int> GetTotalQuantityByMedicationIdAsync(Guid medicationId);
        // Soft delete management
        /// <summary>
        /// Soft delete một medication và các lots liên quan
        /// </summary>
        Task<bool> SoftDeleteAsync(Guid id, Guid deletedBy);

        /// <summary>
        /// Khôi phục medication đã bị soft delete
        /// </summary>
        Task<bool> RestoreAsync(Guid id, Guid restoredBy);

        /// <summary>
        /// Xóa vĩnh viễn medication và các lots liên quan
        /// </summary>
        Task<bool> PermanentDeleteAsync(Guid id);

        /// <summary>
        /// Lấy danh sách các medication đã bị soft delete
        /// </summary>
        Task<PagedList<Medication>> GetSoftDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null);

        /// <summary>
        /// Xóa vĩnh viễn các medication đã soft delete quá thời hạn (mặc định 30 ngày)
        /// </summary>
        Task<int> PermanentDeleteExpiredAsync(int daysToExpire = 30);
    }
}