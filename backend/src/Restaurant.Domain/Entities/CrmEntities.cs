using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class Customer : AuditableEntity
{
    public long RestaurantId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty; // BD Mobile: +8801...
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Notes { get; set; }

    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    public LoyaltyAccount? LoyaltyAccount { get; set; }
}

public class CustomerAddress : BaseEntity
{
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string Label { get; set; } = "Home"; // Home, Office, Other
    public string AddressLine { get; set; } = string.Empty;
    public string District { get; set; } = "Dhaka"; // Bangladesh District
    public string? Upazila { get; set; } // Thana/Upazila
    public string? Area { get; set; } // e.g. Gulshan-1, Dhanmondi 27, Uttara Sector 3
    public string? DeliveryZone { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class LoyaltyAccount : BaseEntity
{
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal PointsBalance { get; set; }
    public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum

    public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
}

public class LoyaltyTransaction : BaseEntity
{
    public long LoyaltyAccountId { get; set; }
    public LoyaltyAccount? LoyaltyAccount { get; set; }
    public string TxnType { get; set; } = "Earn"; // Earn, Redeem, Expire, Adjust
    public decimal Points { get; set; }
    public string ReferenceType { get; set; } = "Order";
    public long? ReferenceId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
