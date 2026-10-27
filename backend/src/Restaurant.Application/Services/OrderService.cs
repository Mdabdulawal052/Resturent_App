using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.Common.Pricing;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class OrderService : IOrderService
{
    private readonly IAppDbContext _db;
    private readonly IKitchenService _kitchenService;
    private readonly IInventoryService _inventoryService;

    public OrderService(
        IAppDbContext db,
        IKitchenService kitchenService,
        IInventoryService inventoryService)
    {
        _db = db;
        _kitchenService = kitchenService;
        _inventoryService = inventoryService;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest req, long userId)
    {
        var orderNo = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        var mushakNo = $"MUSHAK-6.3-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        // Calculate Totals using BangladeshTaxCalculator
        var rawSubtotal = req.Items.Sum(i => i.Qty * i.UnitPrice + i.Modifiers.Sum(m => m.Qty * m.UnitPrice));
        var calc = BangladeshTaxCalculator.Calculate(
            rawSubtotal,
            req.DiscountAmount,
            req.ServiceChargeRate,
            req.VatRate,
            req.IsVatInclusive
        );

        var order = new Order
        {
            RestaurantId = req.RestaurantId,
            BranchId = req.BranchId,
            TerminalId = req.TerminalId,
            OrderNo = orderNo,
            MushakChallanNo = mushakNo,
            OrderType = req.OrderType,
            TableId = req.TableId,
            GuestCoverCount = req.GuestCoverCount > 0 ? req.GuestCoverCount : 1,
            CustomerId = req.CustomerId,
            Status = OrderStatus.Confirmed,
            Subtotal = calc.Subtotal,
            DiscountAmount = calc.DiscountAmount,
            ServiceChargeAmount = calc.ServiceChargeAmount,
            TaxAmount = calc.VatAmount,
            RoundingAmount = calc.RoundingAmount,
            GrandTotal = calc.GrandTotal,
            AggregatorName = req.AggregatorName,
            AggregatorCommissionPercent = req.AggregatorCommissionPercent,
            OrderedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        foreach (var itemReq in req.Items)
        {
            var itemModTotal = itemReq.Modifiers.Sum(m => m.Qty * m.UnitPrice);
            var itemSubtotal = (itemReq.Qty * itemReq.UnitPrice) + itemModTotal;
            var itemNet = Math.Max(0, itemSubtotal - itemReq.DiscountAmount);

            var item = new OrderItem
            {
                ProductId = itemReq.ProductId,
                VariantId = itemReq.VariantId,
                Qty = itemReq.Qty,
                UnitPrice = itemReq.UnitPrice,
                DiscountAmount = itemReq.DiscountAmount,
                NetAmount = itemNet,
                CourseType = string.IsNullOrEmpty(itemReq.CourseType) ? "Main" : itemReq.CourseType,
                GuestSeatNo = itemReq.GuestSeatNo > 0 ? itemReq.GuestSeatNo : 1,
                KitchenStatus = KitchenStatus.New,
                IsSentToKitchen = false,
                Note = itemReq.Note
            };

            foreach (var modReq in itemReq.Modifiers)
            {
                item.Modifiers.Add(new OrderItemModifier
                {
                    ModifierOptionId = modReq.ModifierOptionId,
                    Qty = modReq.Qty,
                    UnitPrice = modReq.UnitPrice
                });
            }

            order.Items.Add(item);
        }

        _db.Orders.Add(order);

        // If dine in, update table status to Occupied
        if (req.TableId.HasValue)
        {
            var table = await _db.Tables.FindAsync(req.TableId.Value);
            if (table != null)
            {
                table.Status = TableStatus.Occupied;
                table.CurrentOrderId = order.Id;
                table.CurrentGuestCount = order.GuestCoverCount;
            }
        }

        await _db.SaveChangesAsync();

        // Audit Log
        _db.AuditLogs.Add(new AuditLog
        {
            RestaurantId = req.RestaurantId,
            BranchId = req.BranchId,
            UserId = userId,
            Action = "CreateOrder",
            EntityName = "Order",
            EntityId = order.Id,
            NewValuesJson = $"{{\"OrderNo\":\"{orderNo}\",\"Mushak\":\"{mushakNo}\",\"GrandTotal\":{calc.GrandTotal}}}"
        });
        await _db.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<OrderDto> AddItemsToOrderAsync(long orderId, List<CreateOrderItemDto> newItems)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found");

        foreach (var itemReq in newItems)
        {
            var itemModTotal = itemReq.Modifiers.Sum(m => m.Qty * m.UnitPrice);
            var itemSubtotal = (itemReq.Qty * itemReq.UnitPrice) + itemModTotal;
            var itemNet = Math.Max(0, itemSubtotal - itemReq.DiscountAmount);

            var item = new OrderItem
            {
                OrderId = order.Id,
                ProductId = itemReq.ProductId,
                VariantId = itemReq.VariantId,
                Qty = itemReq.Qty,
                UnitPrice = itemReq.UnitPrice,
                DiscountAmount = itemReq.DiscountAmount,
                NetAmount = itemNet,
                CourseType = string.IsNullOrEmpty(itemReq.CourseType) ? "Main" : itemReq.CourseType,
                GuestSeatNo = itemReq.GuestSeatNo > 0 ? itemReq.GuestSeatNo : 1,
                KitchenStatus = KitchenStatus.New,
                IsSentToKitchen = false,
                Note = itemReq.Note
            };

            foreach (var modReq in itemReq.Modifiers)
            {
                item.Modifiers.Add(new OrderItemModifier
                {
                    ModifierOptionId = modReq.ModifierOptionId,
                    Qty = modReq.Qty,
                    UnitPrice = modReq.UnitPrice
                });
            }

            order.Items.Add(item);
        }

        // Recalculate totals
        var totalRaw = order.Items.Sum(i => i.NetAmount);
        order.Subtotal = totalRaw;
        var serviceRate = order.Subtotal > 0 ? (order.ServiceChargeAmount / order.Subtotal) * 100 : 10;
        var vatRate = order.Subtotal > 0 ? (order.TaxAmount / order.Subtotal) * 100 : 5;
        var calc = BangladeshTaxCalculator.Calculate(order.Subtotal, order.DiscountAmount, serviceRate, vatRate, true);
        order.ServiceChargeAmount = calc.ServiceChargeAmount;
        order.TaxAmount = calc.VatAmount;
        order.RoundingAmount = calc.RoundingAmount;
        order.GrandTotal = calc.GrandTotal;

        await _db.SaveChangesAsync();
        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<OrderDto> PrintPreBillAsync(long orderId)
    {
        var order = await _db.Orders.FindAsync(orderId) ?? throw new KeyNotFoundException("Order not found");
        order.IsPreBilled = true;

        if (order.TableId.HasValue)
        {
            var table = await _db.Tables.FindAsync(order.TableId.Value);
            if (table != null)
            {
                table.Status = TableStatus.Billed;
            }
        }

        await _db.SaveChangesAsync();
        return await GetOrderByIdAsync(orderId);
    }

    public async Task<(OrderDto OriginalOrder, OrderDto NewSplitOrder)> SplitOrderAsync(SplitOrderRequest req, long userId)
    {
        var originalOrder = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Modifiers)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId)
            ?? throw new KeyNotFoundException("Order not found");

        var newOrderNo = $"{originalOrder.OrderNo}-SPLIT";
        var mushakNo = $"MUSHAK-6.3-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var newOrder = new Order
        {
            RestaurantId = originalOrder.RestaurantId,
            BranchId = originalOrder.BranchId,
            TerminalId = originalOrder.TerminalId,
            OrderNo = newOrderNo,
            MushakChallanNo = mushakNo,
            OrderType = originalOrder.OrderType,
            TableId = originalOrder.TableId,
            GuestCoverCount = 1,
            CustomerId = originalOrder.CustomerId,
            Status = OrderStatus.Confirmed,
            SplitParentOrderId = originalOrder.Id,
            OrderedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        foreach (var itemMove in req.ItemsToMove)
        {
            var item = originalOrder.Items.FirstOrDefault(i => i.Id == itemMove.OrderItemId);
            if (item != null)
            {
                if (item.Qty <= itemMove.Qty)
                {
                    // Move entire line item to new order
                    item.OrderId = 0; // Detach
                    originalOrder.Items.Remove(item);
                    newOrder.Items.Add(item);
                }
                else
                {
                    // Partial qty split
                    item.Qty -= itemMove.Qty;
                    item.NetAmount = item.Qty * item.UnitPrice;

                    var splitItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        Qty = itemMove.Qty,
                        UnitPrice = item.UnitPrice,
                        DiscountAmount = 0,
                        NetAmount = itemMove.Qty * item.UnitPrice,
                        CourseType = item.CourseType,
                        GuestSeatNo = item.GuestSeatNo,
                        KitchenStatus = item.KitchenStatus,
                        IsSentToKitchen = item.IsSentToKitchen,
                        Note = item.Note
                    };
                    newOrder.Items.Add(splitItem);
                }
            }
        }

        _db.Orders.Add(newOrder);

        // Recalculate original order totals
        var origSubtotal = originalOrder.Items.Sum(i => i.NetAmount);
        var origCalc = BangladeshTaxCalculator.Calculate(origSubtotal, originalOrder.DiscountAmount, 10, 5, true);
        originalOrder.Subtotal = origCalc.Subtotal;
        originalOrder.ServiceChargeAmount = origCalc.ServiceChargeAmount;
        originalOrder.TaxAmount = origCalc.VatAmount;
        originalOrder.RoundingAmount = origCalc.RoundingAmount;
        originalOrder.GrandTotal = origCalc.GrandTotal;

        // Calculate new order totals
        var newSubtotal = newOrder.Items.Sum(i => i.NetAmount);
        var newCalc = BangladeshTaxCalculator.Calculate(newSubtotal, 0, 10, 5, true);
        newOrder.Subtotal = newCalc.Subtotal;
        newOrder.ServiceChargeAmount = newCalc.ServiceChargeAmount;
        newOrder.TaxAmount = newCalc.VatAmount;
        newOrder.RoundingAmount = newCalc.RoundingAmount;
        newOrder.GrandTotal = newCalc.GrandTotal;

        await _db.SaveChangesAsync();

        var originalDto = await GetOrderByIdAsync(originalOrder.Id);
        var newDto = await GetOrderByIdAsync(newOrder.Id);
        return (originalDto, newDto);
    }

    public async Task<OrderDto> GetOrderByIdAsync(long orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Variant)
            .Include(o => o.Items).ThenInclude(i => i.Modifiers).ThenInclude(m => m.ModifierOption)
            .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found");

        var table = order.TableId.HasValue ? await _db.Tables.FindAsync(order.TableId.Value) : null;
        var customer = order.CustomerId.HasValue ? await _db.Customers.FindAsync(order.CustomerId.Value) : null;

        return MapOrderToDto(order, table?.TableNo, customer?.Name);
    }

    public async Task<List<OrderDto>> GetActiveOrdersAsync(long branchId)
    {
        var orders = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Variant)
            .Include(o => o.Items).ThenInclude(i => i.Modifiers).ThenInclude(m => m.ModifierOption)
            .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .Where(o => o.BranchId == branchId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync();

        return orders.Select(o => MapOrderToDto(o, null, null)).ToList();
    }

    public async Task<List<KOTDto>> SendToKitchenAsync(long orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Variant)
            .Include(o => o.Items).ThenInclude(i => i.Modifiers).ThenInclude(m => m.ModifierOption)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found");

        var stations = await _db.KitchenStations
            .Where(s => s.BranchId == order.BranchId && s.IsActive)
            .ToListAsync();

        var station = stations.FirstOrDefault() ?? new KitchenStation
        {
            BranchId = order.BranchId,
            Name = "Main Kitchen",
            IsActive = true
        };
        if (station.Id == 0)
        {
            _db.KitchenStations.Add(station);
            await _db.SaveChangesAsync();
        }

        // Only send items that haven't been dispatched yet (Delta KOT)
        var unroutedItems = order.Items.Where(i => !i.IsSentToKitchen || i.KitchenStatus == KitchenStatus.New).ToList();
        if (!unroutedItems.Any()) return await _kitchenService.GetActiveKOTsAsync(order.BranchId);

        var kotNo = $"KOT-{order.Id}-{DateTime.UtcNow:HHmmss}";
        var kot = new KOT
        {
            OrderId = order.Id,
            StationId = station.Id,
            KOTNo = kotNo,
            Status = KitchenStatus.Queued,
            SentAtUtc = DateTime.UtcNow
        };

        foreach (var item in unroutedItems)
        {
            item.KitchenStatus = KitchenStatus.Queued;
            item.IsSentToKitchen = true;
            kot.Items.Add(new KOTItem
            {
                OrderItemId = item.Id,
                Qty = item.Qty,
                Status = KitchenStatus.Queued,
                Note = item.Note
            });
        }

        order.Status = OrderStatus.Preparing;
        if (order.TableId.HasValue)
        {
            var table = await _db.Tables.FindAsync(order.TableId.Value);
            if (table != null && table.Status == TableStatus.Occupied)
            {
                table.Status = TableStatus.KitchenOrderFired;
            }
        }

        _db.KOTs.Add(kot);
        await _db.SaveChangesAsync();

        return await _kitchenService.GetActiveKOTsAsync(order.BranchId);
    }

    public async Task<OrderDto> SettleOrderAsync(SettleOrderRequest req, long userId)
    {
        var order = await _db.Orders
            .Include(o => o.Payments)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId)
            ?? throw new KeyNotFoundException("Order not found");

        foreach (var pReq in req.Payments)
        {
            order.Payments.Add(new Payment
            {
                PaymentMethodId = pReq.PaymentMethodId,
                Amount = pReq.Amount,
                ReferenceNo = pReq.ReferenceNo,
                MfsTrxId = pReq.MfsTrxId,
                CardSlipNo = pReq.CardSlipNo,
                PaidAtUtc = DateTime.UtcNow,
                Status = PaymentStatus.Paid,
                CreatedByUserId = userId
            });
        }

        var totalPaid = order.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        if (totalPaid >= order.GrandTotal)
        {
            order.Status = OrderStatus.Paid;
            order.PaidAtUtc = DateTime.UtcNow;

            // Free table if dine-in
            if (order.TableId.HasValue)
            {
                var table = await _db.Tables.FindAsync(order.TableId.Value);
                if (table != null)
                {
                    table.Status = TableStatus.Available;
                    table.CurrentOrderId = null;
                    table.CurrentGuestCount = 0;
                }
            }

            // Recipe BOM Stock Deduction on Order Completion
            await _inventoryService.DeductInventoryForOrderAsync(order, userId);
        }
        else
        {
            order.Status = OrderStatus.PartiallyPaid;
        }

        await _db.SaveChangesAsync();
        return await GetOrderByIdAsync(order.Id);
    }

    private static OrderDto MapOrderToDto(Order o, string? tableNo, string? customerName)
    {
        var paid = o.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.Amount);
        return new OrderDto(
            o.Id,
            o.BranchId,
            o.OrderNo,
            o.OrderType,
            o.TableId,
            tableNo,
            o.CustomerId,
            customerName,
            o.Status,
            o.Subtotal,
            o.DiscountAmount,
            o.ServiceChargeAmount,
            o.TaxAmount,
            o.RoundingAmount,
            o.GrandTotal,
            paid,
            o.OrderedAtUtc,
            o.PaidAtUtc,
            o.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.Product?.Name ?? "",
                i.VariantId,
                i.Variant?.Name,
                i.Qty,
                i.UnitPrice,
                i.DiscountAmount,
                i.TaxAmount,
                i.NetAmount,
                i.KitchenStatus,
                i.Note,
                i.Modifiers.Select(m => new OrderItemModifierDto(m.Id, m.ModifierOption?.Name ?? "", m.Qty, m.UnitPrice)).ToList(),
                string.IsNullOrEmpty(i.CourseType) ? "Main" : i.CourseType,
                i.GuestSeatNo,
                i.IsSentToKitchen,
                i.KotTokenNo
            )).ToList(),
            o.Payments.Select(p => new PaymentDto(
                p.Id,
                p.PaymentMethodId,
                p.PaymentMethod?.Name ?? "",
                p.Amount,
                p.ReferenceNo,
                p.MfsTrxId,
                p.CardSlipNo,
                p.PaidAtUtc,
                p.Status
            )).ToList(),
            o.GuestCoverCount,
            o.MushakChallanNo,
            o.IsPreBilled,
            o.AggregatorName,
            o.AggregatorCommissionPercent
        );
    }
}
