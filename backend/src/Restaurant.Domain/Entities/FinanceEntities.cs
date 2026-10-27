using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class CashShift : BaseEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long? TerminalId { get; set; }
    public long UserId { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal OpeningCash { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public string? Notes { get; set; }

    // Bangladesh currency note denominations (BDT)
    public int Count1000 { get; set; } = 0;
    public int Count500 { get; set; } = 0;
    public int Count200 { get; set; } = 0;
    public int Count100 { get; set; } = 0;
    public int Count50 { get; set; } = 0;
    public int Count20 { get; set; } = 0;
    public int Count10 { get; set; } = 0;
    public decimal DeclaredCash { get; set; } = 0;

    public ICollection<CashMovement> Movements { get; set; } = new List<CashMovement>();
}

public class CashMovement : BaseEntity
{
    public long CashShiftId { get; set; }
    public CashShift? CashShift { get; set; }
    public CashMovementType MovementType { get; set; } // CashIn, CashOut, Drop
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ExpenseCategory : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. Kitchen Supplies, Utility, Cleaning, Staff Meals
    public bool IsActive { get; set; } = true;
}

public class Expense : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long ExpenseCategoryId { get; set; }
    public ExpenseCategory? ExpenseCategory { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
    public string PaidFrom { get; set; } = "CashDrawer"; // CashDrawer, Bank, PettyCash
    public string? Description { get; set; }
}
