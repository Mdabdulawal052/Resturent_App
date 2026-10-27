using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Enums;

namespace Restaurant.Api.Controllers;

public class TablesController : ApiControllerBase
{
    private readonly ITableService _tableService;

    public TablesController(ITableService tableService) => _tableService = tableService;

    [HttpGet("areas")]
    public async Task<ActionResult<List<DiningAreaDto>>> GetAreas([FromQuery] long branchId = 1)
    {
        return Ok(await _tableService.GetDiningAreasAsync(branchId));
    }

    [HttpPut("{tableId}/status")]
    public async Task<ActionResult<TableDto>> UpdateStatus(long tableId, [FromBody] TableStatus status, [FromQuery] long? orderId = null)
    {
        return Ok(await _tableService.UpdateTableStatusAsync(tableId, status, orderId));
    }

    [HttpPut("layout")]
    public async Task<ActionResult<TableDto>> UpdateLayout([FromBody] UpdateTableLayoutRequest req)
    {
        return Ok(await _tableService.UpdateTableLayoutAsync(req));
    }

    [HttpPost("transfer")]
    public async Task<ActionResult> TransferTable([FromBody] TransferTableRequest req)
    {
        var success = await _tableService.TransferTableAsync(req);
        return success ? Ok(new { message = "Table transferred successfully" }) : BadRequest("Could not transfer table");
    }

    [HttpPost("merge")]
    public async Task<ActionResult> MergeTables([FromBody] MergeTablesRequest req)
    {
        var success = await _tableService.MergeTablesAsync(req);
        return success ? Ok(new { message = "Tables merged successfully" }) : BadRequest("Could not merge tables");
    }

    [HttpPost("areas")]
    public async Task<ActionResult<DiningAreaDto>> CreateArea([FromBody] DiningAreaDto req)
    {
        return Ok(await _tableService.CreateDiningAreaAsync(req.BranchId, req.Name, req.FloorNumber));
    }

    [HttpPost]
    public async Task<ActionResult<TableDto>> CreateTable([FromBody] TableDto req, [FromQuery] long branchId = 1)
    {
        return Ok(await _tableService.CreateTableAsync(branchId, req.DiningAreaId, req.TableNo, req.Capacity, req.Shape, req.PositionX, req.PositionY, req.Width, req.Height));
    }

    [HttpDelete("{tableId}")]
    public async Task<ActionResult> DeleteTable(long tableId)
    {
        var deleted = await _tableService.DeleteTableAsync(tableId);
        return deleted ? Ok(new { message = "Table deleted" }) : NotFound();
    }
}

