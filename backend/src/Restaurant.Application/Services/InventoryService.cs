using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IAppDbContext _db;

    public InventoryService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<InventoryItemDto>> GetInventoryItemsAsync(long restaurantId)
    {
        var items = await _db.InventoryItems
            .Include(i => i.BaseUnit)
            .Where(i => i.RestaurantId == restaurantId && i.IsActive)
            .ToListAsync();

        var balances = await _db.StockBalances.ToListAsync();

        return items.Select(i =>
        {
            var stock = balances.Where(b => b.InventoryItemId == i.Id).Sum(b => b.Quantity);
            var avgCost = balances.Where(b => b.InventoryItemId == i.Id).Select(b => b.AverageCost).FirstOrDefault();
            return new InventoryItemDto(
                i.Id,
                i.SKU,
                i.Name,
                i.BaseUnitId,
                i.BaseUnit?.Code ?? "",
                i.ItemType,
                i.ReorderLevel,
                stock,
                avgCost
            );
        }).ToList();
    }

    public async Task DeductInventoryForOrderAsync(Order order, long userId)
    {
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.BranchId == order.BranchId);
        if (warehouse == null) return;

        foreach (var orderItem in order.Items)
        {
            var recipe = await _db.Recipes
                .Include(r => r.Lines)
                .FirstOrDefaultAsync(r => r.ProductId == orderItem.ProductId && r.IsActive);

            if (recipe == null) continue;

            foreach (var line in recipe.Lines)
            {
                var consumptionQty = orderItem.Qty * line.Quantity * (1m + (line.WastePercent / 100m));

                _db.StockLedger.Add(new StockLedger
                {
                    WarehouseId = warehouse.Id,
                    InventoryItemId = line.InventoryItemId,
                    TxnType = StockTxnType.OrderConsumption,
                    ReferenceType = "Order",
                    ReferenceId = order.Id,
                    QtyIn = 0,
                    QtyOut = consumptionQty,
                    UnitCost = 0,
                    BalanceQty = 0,
                    CreatedAtUtc = DateTime.UtcNow,
                    CreatedByUserId = userId
                });

                var balance = await _db.StockBalances.FindAsync(warehouse.Id, line.InventoryItemId);
                if (balance != null)
                {
                    balance.Quantity -= consumptionQty;
                }
            }
        }
    }
}
