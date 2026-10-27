using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class TableService : ITableService
{
    private readonly IAppDbContext _db;

    public TableService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<DiningAreaDto>> GetDiningAreasAsync(long branchId)
    {
        var areas = await _db.DiningAreas
            .Include(a => a.Tables)
            .Where(a => a.BranchId == branchId && a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        var activeOrders = await _db.Orders
            .Where(o => o.BranchId == branchId && o.TableId != null && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync();

        var orderLookup = activeOrders
            .GroupBy(o => o.TableId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        return areas.Select(a => new DiningAreaDto(
            a.Id,
            a.BranchId,
            a.Name,
            a.FloorNumber,
            a.SortOrder,
            a.Tables.Select(t => {
                var order = orderLookup.GetValueOrDefault(t.Id);
                var minutes = order != null ? (int)(DateTime.UtcNow - order.OrderedAtUtc).TotalMinutes : 0;
                return new TableDto(
                    t.Id,
                    t.DiningAreaId,
                    t.TableNo,
                    t.Capacity,
                    t.Status,
                    t.CurrentOrderId ?? order?.Id,
                    t.CurrentGuestCount > 0 ? t.CurrentGuestCount : (order != null ? order.GuestCoverCount : (int?)null),
                    order?.OrderedAtUtc,
                    t.PositionX,
                    t.PositionY,
                    t.Width > 0 ? t.Width : 120,
                    t.Height > 0 ? t.Height : 120,
                    string.IsNullOrEmpty(t.Shape) ? "square" : t.Shape,
                    t.ColorCode,
                    order?.GrandTotal ?? 0,
                    minutes
                );
            }).ToList()
        )).ToList();
    }

    public async Task<TableDto> UpdateTableStatusAsync(long tableId, TableStatus status, long? orderId = null)
    {
        var table = await _db.Tables.FindAsync(tableId) ?? throw new KeyNotFoundException("Table not found");
        table.Status = status;
        table.CurrentOrderId = orderId;
        await _db.SaveChangesAsync();
        return new TableDto(
            table.Id, table.DiningAreaId, table.TableNo, table.Capacity, table.Status, table.CurrentOrderId,
            table.CurrentGuestCount, null, table.PositionX, table.PositionY, table.Width, table.Height, table.Shape, table.ColorCode, 0, 0
        );
    }

    public async Task<TableDto> UpdateTableLayoutAsync(UpdateTableLayoutRequest req)
    {
        var table = await _db.Tables.FindAsync(req.TableId);
        if (table == null)
        {
            var firstArea = await _db.DiningAreas.FirstOrDefaultAsync();
            var areaId = firstArea?.Id ?? 1;
            var branchId = firstArea?.BranchId ?? 1;

            table = new Domain.Entities.RestaurantTable
            {
                BranchId = branchId,
                DiningAreaId = areaId,
                TableNo = req.TableNo,
                Capacity = req.Capacity,
                Shape = req.Shape,
                PositionX = req.PositionX,
                PositionY = req.PositionY,
                Width = req.Width,
                Height = req.Height,
                Status = TableStatus.Available
            };
            _db.Tables.Add(table);
        }
        else
        {
            table.PositionX = req.PositionX;
            table.PositionY = req.PositionY;
            table.Width = req.Width;
            table.Height = req.Height;
            table.Shape = req.Shape;
            table.Capacity = req.Capacity;
            table.TableNo = req.TableNo;
        }

        await _db.SaveChangesAsync();
        return new TableDto(
            table.Id, table.DiningAreaId, table.TableNo, table.Capacity, table.Status, table.CurrentOrderId,
            table.CurrentGuestCount, null, table.PositionX, table.PositionY, table.Width, table.Height, table.Shape, table.ColorCode, 0, 0
        );
    }

    public async Task<bool> TransferTableAsync(TransferTableRequest req)
    {
        var fromTable = await _db.Tables.FindAsync(req.FromTableId) ?? throw new KeyNotFoundException("Source table not found");
        var toTable = await _db.Tables.FindAsync(req.ToTableId) ?? throw new KeyNotFoundException("Destination table not found");

        var activeOrder = await _db.Orders
            .FirstOrDefaultAsync(o => o.TableId == req.FromTableId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

        if (activeOrder != null)
        {
            activeOrder.TableId = req.ToTableId;
            toTable.CurrentOrderId = activeOrder.Id;
            toTable.Status = fromTable.Status;
            toTable.CurrentGuestCount = fromTable.CurrentGuestCount;

            fromTable.CurrentOrderId = null;
            fromTable.Status = TableStatus.Available;
            fromTable.CurrentGuestCount = 0;

            await _db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> MergeTablesAsync(MergeTablesRequest req)
    {
        var sourceTable = await _db.Tables.FindAsync(req.SourceTableId) ?? throw new KeyNotFoundException("Source table not found");
        var targetTable = await _db.Tables.FindAsync(req.TargetTableId) ?? throw new KeyNotFoundException("Target table not found");

        var sourceOrder = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.TableId == req.SourceTableId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

        var targetOrder = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.TableId == req.TargetTableId && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);

        if (sourceOrder != null && targetOrder != null)
        {
            foreach (var item in sourceOrder.Items)
            {
                item.OrderId = targetOrder.Id;
            }

            targetOrder.Subtotal += sourceOrder.Subtotal;
            targetOrder.DiscountAmount += sourceOrder.DiscountAmount;
            targetOrder.ServiceChargeAmount += sourceOrder.ServiceChargeAmount;
            targetOrder.TaxAmount += sourceOrder.TaxAmount;
            targetOrder.GrandTotal += sourceOrder.GrandTotal;
            targetOrder.GuestCoverCount += sourceOrder.GuestCoverCount;

            sourceOrder.Status = OrderStatus.Cancelled;
            sourceTable.CurrentOrderId = null;
            sourceTable.Status = TableStatus.Available;
            sourceTable.CurrentGuestCount = 0;

            await _db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<DiningAreaDto> CreateDiningAreaAsync(long branchId, string name, int floorNumber)
    {
        var area = new Domain.Entities.DiningArea
        {
            BranchId = branchId,
            Name = name,
            FloorNumber = floorNumber,
            SortOrder = floorNumber,
            IsActive = true
        };
        _db.DiningAreas.Add(area);
        await _db.SaveChangesAsync();

        return new DiningAreaDto(area.Id, area.BranchId, area.Name, area.FloorNumber, area.SortOrder, new List<TableDto>());
    }

    public async Task<TableDto> CreateTableAsync(long branchId, long diningAreaId, string tableNo, int capacity, string shape, double positionX = 100, double positionY = 100, double width = 140, double height = 140)
    {
        var table = new Domain.Entities.RestaurantTable
        {
            BranchId = branchId,
            DiningAreaId = diningAreaId,
            TableNo = tableNo,
            Capacity = capacity,
            Shape = shape,
            PositionX = positionX,
            PositionY = positionY,
            Width = width,
            Height = height,
            Status = TableStatus.Available
        };
        _db.Tables.Add(table);
        await _db.SaveChangesAsync();

        return new TableDto(
            table.Id, table.DiningAreaId, table.TableNo, table.Capacity, table.Status, null, 0, null,
            table.PositionX, table.PositionY, table.Width, table.Height, table.Shape, null, 0, 0
        );
    }

    public async Task<bool> DeleteTableAsync(long tableId)
    {
        var table = await _db.Tables.FindAsync(tableId);
        if (table != null)
        {
            _db.Tables.Remove(table);
            await _db.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
