using Restaurant.Application.DTOs;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services.Interfaces;

public interface IPurchasingService
{
    Task<List<SupplierDto>> GetSuppliersAsync(long restaurantId);
    Task<SupplierDto> CreateSupplierAsync(long restaurantId, CreateSupplierRequest req);
    Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync(long branchId);
    Task<PurchaseOrderDetailDto?> GetPurchaseOrderByIdAsync(long id);
    Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest req, long userId);
    Task<bool> UpdatePOStatusAsync(long poId, PurchaseOrderStatus status, long userId);
    Task<GoodsReceiptDto> ReceiveGoodsAsync(CreateGoodsReceiptRequest req, long userId);
    Task<List<GoodsReceiptDto>> GetGoodsReceiptsAsync(long branchId);
}
