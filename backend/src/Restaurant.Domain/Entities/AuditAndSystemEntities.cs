using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class AuditLog : BaseEntity
{
    public long RestaurantId { get; set; }
    public long? BranchId { get; set; }
    public long? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Void, Refund, DiscountOverride
    public string EntityName { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class NumberSequence : BaseEntity
{
    public long BranchId { get; set; }
    public string DocumentType { get; set; } = string.Empty; // Order, KOT, Invoice, GRN, PO
    public string Prefix { get; set; } = string.Empty;
    public long NextNumber { get; set; } = 1;
}

public class Setting : BaseEntity
{
    public long RestaurantId { get; set; }
    public long? BranchId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
