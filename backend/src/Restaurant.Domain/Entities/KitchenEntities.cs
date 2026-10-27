using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class KitchenStation : BaseEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Grill Station, Fry Station, Beverage / Bar
    public long? PrinterProfileId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<KOT> KOTs { get; set; } = new List<KOT>();
}

public class KOT : AuditableEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public long StationId { get; set; }
    public KitchenStation? Station { get; set; }
    public string KOTNo { get; set; } = string.Empty;
    public KitchenStatus Status { get; set; } = KitchenStatus.Queued;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<KOTItem> Items { get; set; } = new List<KOTItem>();
}

public class KOTItem : BaseEntity
{
    public long KOTId { get; set; }
    public KOT? KOT { get; set; }
    public long OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }
    public decimal Qty { get; set; }
    public KitchenStatus Status { get; set; } = KitchenStatus.Queued;
    public string? Note { get; set; }
}
