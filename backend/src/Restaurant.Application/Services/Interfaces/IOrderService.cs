using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest req, long userId);
    Task<OrderDto> GetOrderByIdAsync(long orderId);
    Task<List<OrderDto>> GetActiveOrdersAsync(long branchId);
    Task<List<KOTDto>> SendToKitchenAsync(long orderId);
    Task<OrderDto> SettleOrderAsync(SettleOrderRequest req, long userId);
    Task<OrderDto> PrintPreBillAsync(long orderId);
    Task<(OrderDto OriginalOrder, OrderDto NewSplitOrder)> SplitOrderAsync(SplitOrderRequest req, long userId);
    Task<OrderDto> AddItemsToOrderAsync(long orderId, List<CreateOrderItemDto> items);
}
