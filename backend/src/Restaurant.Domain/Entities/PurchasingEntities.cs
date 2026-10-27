using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class Supplier : AuditableEntity
{
    public long RestaurantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PurchaseOrder : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string PONo { get; set; } = string.Empty;
    public DateTime PODateUtc { get; set; } = DateTime.UtcNow;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal Total { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

public class PurchaseOrderLine : BaseEntity
{
    public long PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxAmount { get; set; }
}

public class GoodsReceipt : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public long? PurchaseOrderId { get; set; }
    public string GRNNo { get; set; } = string.Empty;
    public DateTime ReceiptDateUtc { get; set; } = DateTime.UtcNow;
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;
    public decimal Total { get; set; }

    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

public class GoodsReceiptLine : BaseEntity
{
    public long GoodsReceiptId { get; set; }
    public GoodsReceipt? GoodsReceipt { get; set; }
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class PurchaseReturn : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string ReturnNo { get; set; } = string.Empty;
    public DateTime ReturnDateUtc { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
}
