namespace Restaurant.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public long? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public long? UpdatedByUserId { get; set; }
    public byte[]? RowVersion { get; set; }
}
