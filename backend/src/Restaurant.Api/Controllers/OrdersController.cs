using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Infrastructure.SignalR;

namespace Restaurant.Api.Controllers;

public class OrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IHubContext<KdsHub> _hub;

    public OrdersController(IOrderService orderService, IHubContext<KdsHub> hub)
    {
        _orderService = orderService;
        _hub = hub;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest req, [FromQuery] long userId = 2)
    {
        var order = await _orderService.CreateOrderAsync(req, userId);
        return Ok(order);
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(long orderId)
    {
        return Ok(await _orderService.GetOrderByIdAsync(orderId));
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<OrderDto>>> GetActiveOrders([FromQuery] long branchId = 1)
    {
        return Ok(await _orderService.GetActiveOrdersAsync(branchId));
    }

    [HttpPost("{orderId}/send-to-kitchen")]
    public async Task<ActionResult<List<KOTDto>>> SendToKitchen(long orderId)
    {
        var kots = await _orderService.SendToKitchenAsync(orderId);
        // Broadcast to KDS via SignalR
        await _hub.Clients.All.SendAsync("KOTsUpdated", kots);
        return Ok(kots);
    }

    [HttpPost("settle")]
    public async Task<ActionResult<OrderDto>> SettleOrder([FromBody] SettleOrderRequest req, [FromQuery] long userId = 2)
    {
        var order = await _orderService.SettleOrderAsync(req, userId);
        return Ok(order);
    }

    [HttpPost("{orderId}/pre-bill")]
    public async Task<ActionResult<OrderDto>> PrintPreBill(long orderId)
    {
        return Ok(await _orderService.PrintPreBillAsync(orderId));
    }

    [HttpPost("split")]
    public async Task<ActionResult> SplitOrder([FromBody] SplitOrderRequest req, [FromQuery] long userId = 2)
    {
        var (orig, split) = await _orderService.SplitOrderAsync(req, userId);
        return Ok(new { originalOrder = orig, newSplitOrder = split });
    }

    [HttpPost("{orderId}/items")]
    public async Task<ActionResult<OrderDto>> AddItems(long orderId, [FromBody] List<CreateOrderItemDto> items)
    {
        return Ok(await _orderService.AddItemsToOrderAsync(orderId, items));
    }
}
