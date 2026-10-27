using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class PurchasingService : IPurchasingService
{
    private readonly IAppDbContext _db;

    public PurchasingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SupplierDto>> GetSuppliersAsync(long restaurantId)
    {
        var list = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.RestaurantId == restaurantId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto(
                s.Id,
                s.Code,
                s.Name,
                s.Phone,
                s.Email,
                s.Address,
                s.OpeningBalance
            ))
            .ToListAsync();

        if (list.Count == 0)
        {
            var sup1 = new Supplier { RestaurantId = restaurantId, Code = "SUP-001", Name = "Bengal Meat Agro Ltd.", Phone = "01711002233", Email = "supply@bengalmeat.com", Address = "Hazaribagh / Savar Plant, Dhaka", OpeningBalance = 0, IsActive = true };
            var sup2 = new Supplier { RestaurantId = restaurantId, Code = "SUP-002", Name = "Pran Dairy & Agro Supplies", Phone = "01811445566", Email = "sales@pranbd.com", Address = "Pran-RFL Centre, Middle Badda, Dhaka", OpeningBalance = 0, IsActive = true };
            var sup3 = new Supplier { RestaurantId = restaurantId, Code = "SUP-003", Name = "Karwan Bazar Fresh Produce Vendor", Phone = "01911778899", Email = "karwanbazar.fresh@gmail.com", Address = "Shed 4, Karwan Bazar Wholesale Market, Dhaka", OpeningBalance = 0, IsActive = true };
            var sup4 = new Supplier { RestaurantId = restaurantId, Code = "SUP-004", Name = "Moulvibazar Spices & Rice Syndicate", Phone = "01611223344", Email = "moulvibazar.spices@gmail.com", Address = "Moulvibazar, Old Dhaka", OpeningBalance = 0, IsActive = true };
            _db.Suppliers.AddRange(sup1, sup2, sup3, sup4);
            await _db.SaveChangesAsync();

            return new List<SupplierDto>
            {
                new(sup1.Id, sup1.Code, sup1.Name, sup1.Phone, sup1.Email, sup1.Address, 0),
                new(sup2.Id, sup2.Code, sup2.Name, sup2.Phone, sup2.Email, sup2.Address, 0),
                new(sup3.Id, sup3.Code, sup3.Name, sup3.Phone, sup3.Email, sup3.Address, 0),
                new(sup4.Id, sup4.Code, sup4.Name, sup4.Phone, sup4.Email, sup4.Address, 0)
            };
        }

        return list;
    }

    public async Task<SupplierDto> CreateSupplierAsync(long restaurantId, CreateSupplierRequest req)
    {
        var supplier = new Supplier
        {
            RestaurantId = restaurantId,
            Code = string.IsNullOrWhiteSpace(req.Code) ? $"SUP-{DateTime.UtcNow.Ticks % 10000:0000}" : req.Code,
            Name = req.Name,
            Phone = req.Phone,
            Email = req.Email,
            Address = req.Address,
            OpeningBalance = req.OpeningBalance,
            IsActive = true
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return new SupplierDto(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.Phone,
            supplier.Email,
            supplier.Address,
            supplier.OpeningBalance
        );
    }

    public async Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(long branchId)
    {
        return await _db.PurchaseOrders
            .AsNoTracking()
            .Where(p => p.BranchId == branchId)
            .Include(p => p.Supplier)
            .Include(p => p.Lines)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PurchaseOrderDto(
                p.Id,
                p.BranchId,
                p.SupplierId,
                p.Supplier != null ? p.Supplier.Name : "Unknown Supplier",
                p.PONo,
                p.PODateUtc,
                p.Status,
                p.Total,
                p.Lines.Count
            ))
            .ToListAsync();
    }

    public async Task<PurchaseOrderDetailDto?> GetPurchaseOrderByIdAsync(long id)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Lines)
                .ThenInclude(l => l.InventoryItem)
                    .ThenInclude(i => i!.BaseUnit)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (po == null) return null;

        var lines = po.Lines.Select(l => new PurchaseOrderLineDto(
            l.Id,
            l.InventoryItemId,
            l.InventoryItem != null ? l.InventoryItem.Name : "Item",
            l.InventoryItem?.BaseUnit?.Code ?? "KG",
            l.Qty,
            l.UnitCost,
            l.TaxAmount,
            (l.Qty * l.UnitCost) + l.TaxAmount
        )).ToList();

        return new PurchaseOrderDetailDto(
            po.Id,
            po.BranchId,
            po.SupplierId,
            po.Supplier != null ? po.Supplier.Name : "Unknown Supplier",
            po.Supplier?.Phone,
            po.Supplier?.Address,
            po.PONo,
            po.PODateUtc,
            po.Status,
            po.Total,
            lines
        );
    }

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest req, long userId)
    {
        var count = await _db.PurchaseOrders.CountAsync() + 1;
        var poNo = $"PO-{DateTime.UtcNow:yyyyMM}-{count:D4}";

        decimal total = 0;
        var po = new PurchaseOrder
        {
            BranchId = req.BranchId,
            SupplierId = req.SupplierId,
            PONo = poNo,
            PODateUtc = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Draft,
            CreatedByUserId = userId
        };

        foreach (var l in req.Lines)
        {
            var lineTotal = (l.Qty * l.UnitCost) + l.TaxAmount;
            total += lineTotal;
            po.Lines.Add(new PurchaseOrderLine
            {
                InventoryItemId = l.InventoryItemId,
                Qty = l.Qty,
                UnitCost = l.UnitCost,
                TaxAmount = l.TaxAmount
            });
        }

        po.Total = total;
        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        var supplierName = await _db.Suppliers
            .Where(s => s.Id == req.SupplierId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync() ?? "Supplier";

        return new PurchaseOrderDto(
            po.Id,
            po.BranchId,
            po.SupplierId,
            supplierName,
            po.PONo,
            po.PODateUtc,
            po.Status,
            po.Total,
            po.Lines.Count
        );
    }

    public async Task<bool> UpdatePOStatusAsync(long poId, PurchaseOrderStatus status, long userId)
    {
        var po = await _db.PurchaseOrders.FindAsync(poId);
        if (po == null) return false;

        po.Status = status;
        po.UpdatedByUserId = userId;
        po.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<GoodsReceiptDto> ReceiveGoodsAsync(CreateGoodsReceiptRequest req, long userId)
    {
        var count = await _db.GoodsReceipts.CountAsync() + 1;
        var grnNo = $"GRN-{DateTime.UtcNow:yyyyMM}-{count:D4}";

        decimal total = 0;
        var grn = new GoodsReceipt
        {
            BranchId = req.BranchId,
            SupplierId = req.SupplierId,
            PurchaseOrderId = req.PurchaseOrderId,
            GRNNo = grnNo,
            ReceiptDateUtc = DateTime.UtcNow,
            Status = GoodsReceiptStatus.Posted,
            CreatedByUserId = userId
        };

        // Determine branch warehouse
        var warehouse = await _db.Warehouses.FirstOrDefaultAsync(w => w.BranchId == req.BranchId && w.IsActive);
        var warehouseId = warehouse?.Id ?? 1;

        foreach (var l in req.Lines)
        {
            var lineTotal = l.Qty * l.UnitCost;
            total += lineTotal;
            grn.Lines.Add(new GoodsReceiptLine
            {
                InventoryItemId = l.InventoryItemId,
                Qty = l.Qty,
                UnitCost = l.UnitCost,
                BatchNo = l.BatchNo ?? $"BATCH-{DateTime.UtcNow:MMdd}",
                ExpiryDate = l.ExpiryDate
            });

            // Update StockBalance
            var balance = await _db.StockBalances
                .FirstOrDefaultAsync(sb => sb.WarehouseId == warehouseId && sb.InventoryItemId == l.InventoryItemId);

            if (balance != null)
            {
                // Weighted average cost update
                var totalValuation = (balance.Quantity * balance.AverageCost) + (l.Qty * l.UnitCost);
                var newQty = balance.Quantity + l.Qty;
                balance.Quantity = newQty;
                if (newQty > 0)
                {
                    balance.AverageCost = Math.Round(totalValuation / newQty, 2);
                }
            }
            else
            {
                _db.StockBalances.Add(new StockBalance
                {
                    WarehouseId = warehouseId,
                    InventoryItemId = l.InventoryItemId,
                    Quantity = l.Qty,
                    AverageCost = l.UnitCost
                });
            }

            // Ledger Inward Entry
            _db.StockLedger.Add(new StockLedger
            {
                WarehouseId = warehouseId,
                InventoryItemId = l.InventoryItemId,
                TxnType = StockTxnType.PurchaseReceipt,
                ReferenceType = "GRN",
                ReferenceId = grn.Id,
                QtyIn = l.Qty,
                QtyOut = 0,
                UnitCost = l.UnitCost,
                BalanceQty = (balance?.Quantity ?? 0) + l.Qty,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByUserId = userId
            });
        }

        grn.Total = total;
        _db.GoodsReceipts.Add(grn);

        // If linked to PO, mark PO as Received
        if (req.PurchaseOrderId.HasValue)
        {
            var po = await _db.PurchaseOrders.FindAsync(req.PurchaseOrderId.Value);
            if (po != null)
            {
                po.Status = PurchaseOrderStatus.Received;
                po.UpdatedByUserId = userId;
                po.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        var supplierName = await _db.Suppliers
            .Where(s => s.Id == req.SupplierId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync() ?? "Supplier";

        return new GoodsReceiptDto(
            grn.Id,
            grn.BranchId,
            grn.SupplierId,
            supplierName,
            grn.GRNNo,
            grn.ReceiptDateUtc,
            grn.Status,
            grn.Total
        );
    }

    public async Task<List<GoodsReceiptDto>> GetGoodsReceiptsAsync(long branchId)
    {
        return await _db.GoodsReceipts
            .AsNoTracking()
            .Where(g => g.BranchId == branchId)
            .Include(g => g.Supplier)
            .OrderByDescending(g => g.ReceiptDateUtc)
            .Select(g => new GoodsReceiptDto(
                g.Id,
                g.BranchId,
                g.SupplierId,
                g.Supplier != null ? g.Supplier.Name : "Supplier",
                g.GRNNo,
                g.ReceiptDateUtc,
                g.Status,
                g.Total
            ))
            .ToListAsync();
    }
}
