namespace Repositories.Interfaces
{
    public interface IBaseEntity<TKey>
    {
        TKey Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        Guid? CreatedBy { get; set; }
        Guid? UpdatedBy { get; set; }
    }

    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        Guid? DeletedBy { get; set; }
        DateTime? DeletedAt { get; set; }
    }

    public interface IBaseAuditableEntity<TKey> : IBaseEntity<TKey>, ISoftDeletable
    {
    }
}
