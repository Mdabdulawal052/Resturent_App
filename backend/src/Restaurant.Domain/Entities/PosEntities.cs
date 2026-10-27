using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class RestaurantEntity : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? BIN { get; set; } // Business Identification Number (Bangladesh NBR VAT)
    public string? TIN { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string CurrencyCode { get; set; } = "BDT";
    public string TimeZoneId { get; set; } = "Asia/Dhaka";
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();
}

public class Branch : AuditableEntity
{
    public long RestaurantId { get; set; }
    public RestaurantEntity? Restaurant { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? BIN { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Terminal> Terminals { get; set; } = new List<Terminal>();
    public ICollection<DiningArea> DiningAreas { get; set; } = new List<DiningArea>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Terminal : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DeviceKeyHash { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DiningArea : BaseEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string Name { get; set; } = string.Empty;
    public int FloorNumber { get; set; } = 1;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
}

public class RestaurantTable : BaseEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long DiningAreaId { get; set; }
    public DiningArea? DiningArea { get; set; }
    public string TableNo { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4;
    public int CurrentGuestCount { get; set; } = 0;
    public double PositionX { get; set; } = 50;
    public double PositionY { get; set; } = 50;
    public double Width { get; set; } = 120;
    public double Height { get; set; } = 120;
    public string Shape { get; set; } = "square"; // round, square, rectangle
    public string? ColorCode { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Available;
    public long? CurrentOrderId { get; set; }

    public ICollection<TableSession> Sessions { get; set; } = new List<TableSession>();
}

public class TableSession : BaseEntity
{
    public long TableId { get; set; }
    public RestaurantTable? Table { get; set; }
    public long? OrderId { get; set; }
    public int GuestCount { get; set; }
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PreBillPrintedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}
