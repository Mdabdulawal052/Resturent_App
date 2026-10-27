namespace Restaurant.Domain.Enums;

public enum OrderType : byte
{
    DineIn = 1,
    Takeaway = 2,
    Delivery = 3,
    QuickSale = 4
}

public enum OrderStatus : byte
{
    Draft = 1,
    Held = 2,
    Confirmed = 3,
    Preparing = 4,
    Ready = 5,
    Served = 6,
    PartiallyPaid = 7,
    Paid = 8,
    Completed = 8,
    Cancelled = 9,
    Refunded = 10
}

public enum KitchenStatus : byte
{
    New = 0,
    Queued = 1,
    Preparing = 2,
    Ready = 3,
    Served = 4,
    Cancelled = 5
}

public enum PaymentMethodType : byte
{
    Cash = 1,
    Card = 2,
    MFS = 3,
    Bank = 4,
    Other = 5
}

public enum PaymentStatus : byte
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Voided = 3,
    Refunded = 4
}

public enum ItemType : byte
{
    Ingredient = 1,
    Packaging = 2,
    Finished = 3,
    Other = 4
}

public enum StockTxnType : byte
{
    PurchaseReceipt = 1,
    OrderConsumption = 2,
    ManualAdjustment = 3,
    Wastage = 4,
    TransferIn = 5,
    TransferOut = 6,
    PurchaseReturn = 7
}

public enum PurchaseOrderStatus : byte
{
    Draft = 1,
    Approved = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}

public enum GoodsReceiptStatus : byte
{
    Draft = 1,
    Posted = 2,
    Cancelled = 3
}

public enum TableStatus : byte
{
    Available = 1,
    Occupied = 2,
    Billed = 3,
    Reserved = 4,
    Cleaning = 5,
    KitchenOrderFired = 6
}

public enum CashMovementType : byte
{
    CashIn = 1,
    CashOut = 2,
    Drop = 3
}
