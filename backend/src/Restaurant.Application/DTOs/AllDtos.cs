using Restaurant.Domain.Enums;

namespace Restaurant.Application.DTOs;

// --- Auth DTOs ---
public record LoginRequest(string UserName, string Password);
public record PinLoginRequest(string Pin, long? BranchId);
public record AuthResponse(string Token, long UserId, string UserName, string DisplayName, string Role, long RestaurantId, long? BranchId);

// --- Menu DTOs ---
public record CategoryDto(long Id, string Name, int SortOrder, long? ParentId, bool IsActive);
public record ProductVariantDto(long Id, string Name, string? SKU, decimal PriceDelta, bool IsActive);
public record ModifierOptionDto(long Id, string Name, decimal PriceDelta, bool IsActive);
public record ModifierDto(long Id, string Name, bool IsRequired, int MinSelect, int MaxSelect, List<ModifierOptionDto> Options);
public record ProductDto(
    long Id,
    long CategoryId,
    string CategoryName,
    string? SKU,
    string Name,
    string? ShortName,
    string? Description,
    decimal BasePrice,
    decimal? CostPrice,
    bool IsTaxInclusive,
    bool IsActive,
    List<ProductVariantDto> Variants,
    List<ModifierDto> Modifiers
);
public record ComboDto(long Id, string Name, decimal Price, bool IsActive);

// --- Dining / Table DTOs ---
public record DiningAreaDto(long Id, long BranchId, string Name, int FloorNumber, int SortOrder, List<TableDto> Tables);
public record TableDto(
    long Id,
    long DiningAreaId,
    string TableNo,
    int Capacity,
    TableStatus Status,
    long? CurrentOrderId,
    int? GuestCount,
    DateTime? OpenedAtUtc,
    double PositionX = 50,
    double PositionY = 50,
    double Width = 120,
    double Height = 120,
    string Shape = "square",
    string? ColorCode = null,
    decimal? ActiveOrderTotal = 0,
    int ActiveOrderMinutes = 0
);
public record OpenTableSessionRequest(long TableId, int GuestCount);
public record UpdateTableLayoutRequest(long TableId, double PositionX, double PositionY, double Width, double Height, string Shape, int Capacity, string TableNo);
public record TransferTableRequest(long FromTableId, long ToTableId);
public record MergeTablesRequest(long SourceTableId, long TargetTableId);

// --- Order DTOs ---
public record CreateOrderItemModifierDto(long ModifierOptionId, decimal Qty, decimal UnitPrice);
public record CreateOrderItemDto(
    long ProductId,
    long? VariantId,
    decimal Qty,
    decimal UnitPrice,
    decimal DiscountAmount,
    string? Note,
    List<CreateOrderItemModifierDto> Modifiers,
    string CourseType = "Main",
    int GuestSeatNo = 1
);
public record CreateOrderRequest(
    long RestaurantId,
    long BranchId,
    long? TerminalId,
    OrderType OrderType,
    long? TableId,
    long? CustomerId,
    List<CreateOrderItemDto> Items,
    decimal DiscountAmount,
    decimal ServiceChargeRate,
    decimal VatRate,
    bool IsVatInclusive,
    int GuestCoverCount = 1,
    string? AggregatorName = null,
    decimal AggregatorCommissionPercent = 0
);

public record OrderItemModifierDto(long Id, string OptionName, decimal Qty, decimal UnitPrice);
public record OrderItemDto(
    long Id,
    long ProductId,
    string ProductName,
    long? VariantId,
    string? VariantName,
    decimal Qty,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal NetAmount,
    KitchenStatus KitchenStatus,
    string? Note,
    List<OrderItemModifierDto> Modifiers,
    string CourseType = "Main",
    int GuestSeatNo = 1,
    bool IsSentToKitchen = false,
    int KotTokenNo = 0
);
public record OrderDto(
    long Id,
    long BranchId,
    string OrderNo,
    OrderType OrderType,
    long? TableId,
    string? TableNo,
    long? CustomerId,
    string? CustomerName,
    OrderStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ServiceChargeAmount,
    decimal TaxAmount,
    decimal RoundingAmount,
    decimal GrandTotal,
    decimal PaidAmount,
    DateTime OrderedAtUtc,
    DateTime? PaidAtUtc,
    List<OrderItemDto> Items,
    List<PaymentDto> Payments,
    int GuestCoverCount = 1,
    string? MushakChallanNo = null,
    bool IsPreBilled = false,
    string? AggregatorName = null,
    decimal AggregatorCommissionPercent = 0
);

public record SplitItemMoveDto(long OrderItemId, decimal Qty);
public record SplitOrderRequest(long OrderId, List<SplitItemMoveDto> ItemsToMove);

// --- Kitchen / KDS DTOs ---
public record KOTItemDto(long Id, long OrderItemId, string ProductName, string? VariantName, decimal Qty, KitchenStatus Status, string? Note, List<string> Modifiers, string CourseType = "Main");
public record KOTDto(
    long Id,
    long OrderId,
    string OrderNo,
    string? TableNo,
    OrderType OrderType,
    long StationId,
    string StationName,
    string KOTNo,
    KitchenStatus Status,
    DateTime SentAtUtc,
    List<KOTItemDto> Items
);
public record UpdateKOTStatusRequest(KitchenStatus Status);

// --- Payment & Settlement DTOs ---
public record PaymentItemRequest(
    long PaymentMethodId,
    decimal Amount,
    string? ReferenceNo = null,
    string? MfsTrxId = null,
    string? CardSlipNo = null
);
public record SettleOrderRequest(long OrderId, List<PaymentItemRequest> Payments);
public record PaymentMethodDto(long Id, string Code, string Name, PaymentMethodType MethodType, bool IsCash, bool IsActive);
public record PaymentDto(long Id, long PaymentMethodId, string MethodName, decimal Amount, string? ReferenceNo, string? MfsTrxId, string? CardSlipNo, DateTime PaidAtUtc, PaymentStatus Status);
public record RefundRequest(long PaymentId, decimal Amount, string Reason);

// --- Cash Shifts DTOs ---
public record OpenShiftRequest(long BranchId, long? TerminalId, decimal OpeningCash);
public record CloseShiftRequest(
    long ShiftId,
    decimal ClosingCash,
    string? Notes,
    int Count1000 = 0,
    int Count500 = 0,
    int Count200 = 0,
    int Count100 = 0,
    int Count50 = 0,
    int Count20 = 0,
    int Count10 = 0,
    decimal DeclaredCash = 0
);
public record CashMovementRequest(long ShiftId, CashMovementType MovementType, decimal Amount, string Reason);
public record CashShiftDto(
    long Id,
    long BranchId,
    long UserId,
    string UserName,
    DateTime OpenedAtUtc,
    decimal OpeningCash,
    DateTime? ClosedAtUtc,
    decimal? ClosingCash,
    decimal? ExpectedCash,
    decimal? Variance,
    string? Notes,
    int Count1000,
    int Count500,
    int Count200,
    int Count100,
    int Count50,
    int Count20,
    int Count10,
    decimal DeclaredCash,
    List<CashMovementDto> Movements
);
public record CashMovementDto(long Id, CashMovementType MovementType, decimal Amount, string Reason, DateTime CreatedAtUtc);

// --- Inventory & Recipe DTOs ---
public record UnitDto(long Id, string Code, string Name, decimal ConversionFactor);
public record InventoryItemDto(long Id, string? SKU, string Name, long BaseUnitId, string UnitCode, ItemType ItemType, decimal ReorderLevel, decimal CurrentStock, decimal AverageCost);
public record StockAdjustmentRequest(long WarehouseId, long InventoryItemId, decimal Quantity, decimal UnitCost, string Reason);
public record StockTransferRequest(long SourceWarehouseId, long TargetWarehouseId, long InventoryItemId, decimal Quantity);
public record RecipeLineDto(long Id, long InventoryItemId, string ItemName, string UnitCode, decimal Quantity, decimal WastePercent, decimal ComponentCost);
public record RecipeDto(
    long Id,
    long ProductId,
    string ProductName,
    int VersionNo,
    decimal YieldQty,
    long UnitId,
    string UnitCode,
    decimal TotalCost,
    decimal SellingPrice,
    decimal GrossMarginPercent,
    List<RecipeLineDto> Lines
);
public record CreateRecipeRequest(long ProductId, decimal YieldQty, long UnitId, List<CreateRecipeLineRequest> Lines);
public record CreateRecipeLineRequest(long InventoryItemId, decimal Quantity, decimal WastePercent);

// --- Purchasing DTOs ---
public record SupplierDto(long Id, string Code, string Name, string? Phone, string? Email, string? Address, decimal OpeningBalance);
public record CreateSupplierRequest(string Code, string Name, string? Phone, string? Email, string? Address, decimal OpeningBalance);
public record CreatePurchaseOrderLineRequest(long InventoryItemId, decimal Qty, decimal UnitCost, decimal TaxAmount);
public record CreatePurchaseOrderRequest(long BranchId, long SupplierId, List<CreatePurchaseOrderLineRequest> Lines);
public record PurchaseOrderLineDto(long Id, long InventoryItemId, string ItemName, string UnitCode, decimal Qty, decimal UnitCost, decimal TaxAmount, decimal LineTotal);
public record PurchaseOrderDto(long Id, long BranchId, long SupplierId, string SupplierName, string PONo, DateTime PODateUtc, PurchaseOrderStatus Status, decimal Total, int LinesCount = 0);
public record PurchaseOrderDetailDto(long Id, long BranchId, long SupplierId, string SupplierName, string? SupplierPhone, string? SupplierAddress, string PONo, DateTime PODateUtc, PurchaseOrderStatus Status, decimal Total, List<PurchaseOrderLineDto> Lines);

public record CreateGoodsReceiptLineRequest(long InventoryItemId, decimal Qty, decimal UnitCost, string? BatchNo, DateTime? ExpiryDate);
public record CreateGoodsReceiptRequest(long BranchId, long SupplierId, long? PurchaseOrderId, List<CreateGoodsReceiptLineRequest> Lines);
public record GoodsReceiptDto(long Id, long BranchId, long SupplierId, string SupplierName, string GRNNo, DateTime ReceiptDateUtc, GoodsReceiptStatus Status, decimal Total);

// --- CRM DTOs ---
public record CustomerDto(
    long Id,
    string? Code,
    string Name,
    string Phone,
    string? Email,
    decimal LoyaltyPoints,
    string LoyaltyTier,
    List<CustomerAddressDto> Addresses
);
public record CustomerAddressDto(long Id, string Label, string AddressLine, string District, string? Upazila, string? Area);
public record CreateCustomerRequest(string Name, string Phone, string? Email, string? AddressLine, string? District, string? Area);

// --- Expenses DTOs ---
public record ExpenseCategoryDto(long Id, string Name);
public record ExpenseDto(long Id, long BranchId, long ExpenseCategoryId, string CategoryName, decimal Amount, DateTime ExpenseDateUtc, string PaidFrom, string? Description);
public record CreateExpenseRequest(long BranchId, long ExpenseCategoryId, decimal Amount, string PaidFrom, string? Description);

// --- Tax & NBR DTOs ---
public record TaxCategoryDto(long Id, string Code, string Name, decimal Rate, bool IsInclusive);
public record TaxInvoiceDto(long Id, long OrderId, string InvoiceNo, DateTime InvoiceDateUtc, string? CustomerName, string? CustomerBIN, decimal TaxableAmount, decimal VATAmount, decimal TotalAmount);

// --- Reports DTOs ---
public record SalesSummaryDto(
    DateTime Date,
    int TotalOrders,
    decimal GrossSales,
    decimal NetSales,
    decimal TotalTax,
    decimal TotalDiscount,
    decimal TotalServiceCharge,
    decimal CashPayments,
    decimal DigitalPayments // bKash, Nagad, Cards
);
public record HourlySalesDto(int Hour, int OrderCount, decimal TotalSales);
public record TopProductDto(long ProductId, string ProductName, decimal QuantitySold, decimal Revenue);
public record VatReportDto(string TaxCode, decimal TaxRate, decimal TaxableSales, decimal VatCollected);
public record FoodCostReportDto(long ProductId, string ProductName, decimal SellingPrice, decimal RecipeCost, decimal GrossProfit, decimal MarginPercent);

// --- Audit DTOs ---
public record AuditLogDto(long Id, long? UserId, string Action, string EntityName, long? EntityId, string? OldValuesJson, string? NewValuesJson, DateTime CreatedAtUtc);

// --- Booking DTOs ---
public record TableBookingDto(
    long Id,
    long BranchId,
    string BookingNo,
    string BookingType, // "Table" or "Chair"
    long TableId,
    string TableNo,
    int? SeatNumber,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    int GuestCount,
    DateTime BookingDate,
    string TimeSlot,
    string Status,
    string? SpecialRequests,
    decimal DepositAmount,
    bool IsDepositPaid,
    DateTime CreatedAtUtc
);

public record CreateBookingRequest(
    long BranchId,
    string BookingType,
    long TableId,
    string TableNo,
    int? SeatNumber,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    int GuestCount,
    DateTime BookingDate,
    string TimeSlot,
    string? SpecialRequests,
    decimal DepositAmount = 0,
    bool IsDepositPaid = false,
    string? Status = "Pending"
);

public record UpdateBookingStatusRequest(string Status);

