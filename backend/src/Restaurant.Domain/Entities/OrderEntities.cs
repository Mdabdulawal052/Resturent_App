using Restaurant.Domain.Common;
using Restaurant.Domain.Enums;

namespace Restaurant.Domain.Entities;

public class Order : AuditableEntity
{
    public long RestaurantId { get; set; }
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public long? TerminalId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string? MushakChallanNo { get; set; } // NBR Mushak 6.3 Challan No
    public OrderType OrderType { get; set; }
    public long? TableId { get; set; }
    public int GuestCoverCount { get; set; } = 1;
    public long? CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ServiceChargeAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal RoundingAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public bool IsPreBilled { get; set; } = false;
    public long? SplitParentOrderId { get; set; }
    public string? AggregatorName { get; set; } // Foodpanda, Pathao
    public decimal AggregatorCommissionPercent { get; set; } = 0;
    public DateTime OrderedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<OrderDiscount> Discounts { get; set; } = new List<OrderDiscount>();
}

public class OrderItem : BaseEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public long? VariantId { get; set; }
    public ProductVariant? Variant { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string CourseType { get; set; } = "Main"; // Starter, Main, Dessert, Beverage
    public int GuestSeatNo { get; set; } = 1;
    public bool IsSentToKitchen { get; set; } = false;
    public int KotTokenNo { get; set; } = 0;
    public KitchenStatus KitchenStatus { get; set; } = KitchenStatus.New;
    public string? Note { get; set; }

    public ICollection<OrderItemModifier> Modifiers { get; set; } = new List<OrderItemModifier>();
}

public class OrderItemModifier : BaseEntity
{
    public long OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }
    public long ModifierOptionId { get; set; }
    public ModifierOption? ModifierOption { get; set; }
    public decimal Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class OrderDiscount : BaseEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public string DiscountType { get; set; } = "Percentage"; // Percentage or FixedAmount
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public long? ApprovedByUserId { get; set; }
}

public class PaymentMethod : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PaymentMethodType MethodType { get; set; } // Cash, Card, MFS (bKash/Nagad/Rocket), Bank
    public bool IsCash { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Payment : BaseEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public long PaymentMethodId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; } // e.g. bKash/Nagad TrxId, Card Approval Code
    public string? MfsTrxId { get; set; }
    public string? CardSlipNo { get; set; }
    public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;
    public long CreatedByUserId { get; set; }
}

public class Refund : BaseEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public long PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime RefundedAtUtc { get; set; } = DateTime.UtcNow;
    public long ApprovedByUserId { get; set; }
}
