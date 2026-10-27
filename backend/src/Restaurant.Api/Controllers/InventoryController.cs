using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class InventoryController : ApiControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService) => _inventoryService = inventoryService;

    [HttpGet("items")]
    public async Task<ActionResult<List<InventoryItemDto>>> GetItems([FromQuery] long restaurantId = 1)
    {
        return Ok(await _inventoryService.GetInventoryItemsAsync(restaurantId));
    }
}
