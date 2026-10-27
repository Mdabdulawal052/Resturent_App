using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class TaxCategory : BaseEntity
{
    public long RestaurantId { get; set; }
    public string Code { get; set; } = string.Empty; // e.g. VAT-5, VAT-7.5, VAT-15
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; } // e.g. 5.00, 7.50, 15.00
    public bool IsInclusive { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class TaxInvoice : BaseEntity
{
    public long OrderId { get; set; }
    public Order? Order { get; set; }
    public string InvoiceNo { get; set; } = string.Empty; // Bangladesh NBR / Mushak invoice number
    public DateTime InvoiceDateUtc { get; set; } = DateTime.UtcNow;
    public string? CustomerName { get; set; }
    public string? CustomerBIN { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal VATAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
