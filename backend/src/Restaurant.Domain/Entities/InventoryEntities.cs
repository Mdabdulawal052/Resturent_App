using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class Unit : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. KG, G, L, ML, PCS
    public string Name { get; set; } = string.Empty;
    public long? BaseUnitId { get; set; }
    public decimal ConversionFactor { get; set; } = 1;
}

public class InventoryItem : AuditableEntity
{
    public long RestaurantId { get; set; }
    public string? SKU { get; set; }
    public string Name { get; set; } = string.Empty;
    public long BaseUnitId { get; set; }
    public Unit? BaseUnit { get; set; }
    public ItemType ItemType { get; set; } // Ingredient, Packaging, Finished, Other
    public decimal ReorderLevel { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public class Warehouse : BaseEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class StockBalance : BaseEntity
{
    public long WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
}

public class StockLedger : BaseEntity
{
    public long WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public StockTxnType TxnType { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // e.g. "Order", "GRN", "Adjustment", "Wastage"
    public long? ReferenceId { get; set; }
    public decimal QtyIn { get; set; }
    public decimal QtyOut { get; set; }
    public decimal UnitCost { get; set; }
    public decimal BalanceQty { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public long CreatedByUserId { get; set; }
}

public class Recipe : AuditableEntity
{
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public int VersionNo { get; set; } = 1;
    public decimal YieldQty { get; set; } = 1; // Yield per order portion
    public long UnitId { get; set; }
    public Unit? Unit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RecipeLine> Lines { get; set; } = new List<RecipeLine>();
}

public class RecipeLine : BaseEntity
{
    public long RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public long InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }
    public decimal Quantity { get; set; } // Quantity consumed in base unit
    public decimal WastePercent { get; set; } = 0; // Extra % lost during preparation
}
