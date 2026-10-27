using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Enums;

namespace Restaurant.Api.Controllers;

public class PurchasingController : ApiControllerBase
{
    private readonly IPurchasingService _purchasingService;

    public PurchasingController(IPurchasingService purchasingService)
    {
        _purchasingService = purchasingService;
    }

    [HttpGet("suppliers")]
    public async Task<ActionResult<List<SupplierDto>>> GetSuppliers([FromQuery] long restaurantId = 1)
    {
        return Ok(await _purchasingService.GetSuppliersAsync(restaurantId));
    }

    [HttpPost("suppliers")]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromQuery] long restaurantId = 1, [FromBody] CreateSupplierRequest req = null!)
    {
        var result = await _purchasingService.CreateSupplierAsync(restaurantId, req);
        return CreatedAtAction(nameof(GetSuppliers), new { restaurantId }, result);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetPurchaseOrders([FromQuery] long branchId = 1)
    {
        return Ok(await _purchasingService.GetPurchaseOrdersAsync(branchId));
    }

    [HttpGet("orders/{id}")]
    public async Task<ActionResult<PurchaseOrderDetailDto>> GetPurchaseOrderById(long id)
    {
        var po = await _purchasingService.GetPurchaseOrderByIdAsync(id);
        if (po == null) return NotFound(new { message = $"Purchase Order #{id} not found." });
        return Ok(po);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<PurchaseOrderDto>> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest req, [FromQuery] long userId = 1)
    {
        var result = await _purchasingService.CreatePurchaseOrderAsync(req, userId);
        return CreatedAtAction(nameof(GetPurchaseOrderById), new { id = result.Id }, result);
    }

    [HttpPut("orders/{id}/status")]
    public async Task<IActionResult> UpdatePOStatus(long id, [FromBody] PurchaseOrderStatus status, [FromQuery] long userId = 1)
    {
        var success = await _purchasingService.UpdatePOStatusAsync(id, status, userId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("orders/receive")]
    public async Task<ActionResult<GoodsReceiptDto>> ReceiveGoods([FromBody] CreateGoodsReceiptRequest req, [FromQuery] long userId = 1)
    {
        var grn = await _purchasingService.ReceiveGoodsAsync(req, userId);
        return Ok(grn);
    }

    [HttpGet("receipts")]
    public async Task<ActionResult<List<GoodsReceiptDto>>> GetGoodsReceipts([FromQuery] long branchId = 1)
    {
        return Ok(await _purchasingService.GetGoodsReceiptsAsync(branchId));
    }
}
