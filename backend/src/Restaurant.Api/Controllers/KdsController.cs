using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Infrastructure.SignalR;

namespace Restaurant.Api.Controllers;

public class KdsController : ApiControllerBase
{
    private readonly IKitchenService _kitchenService;
    private readonly IHubContext<KdsHub> _hub;

    public KdsController(IKitchenService kitchenService, IHubContext<KdsHub> hub)
    {
        _kitchenService = kitchenService;
        _hub = hub;
    }

    [HttpGet("tickets")]
    public async Task<ActionResult<List<KOTDto>>> GetTickets([FromQuery] long branchId = 1, [FromQuery] long? stationId = null)
    {
        return Ok(await _kitchenService.GetActiveKOTsAsync(branchId, stationId));
    }

    [HttpPut("tickets/{kotId}/status")]
    public async Task<ActionResult<KOTDto>> UpdateKOTStatus(long kotId, [FromBody] UpdateKOTStatusRequest req)
    {
        var kot = await _kitchenService.UpdateKOTStatusAsync(kotId, req.Status);
        var activeKots = await _kitchenService.GetActiveKOTsAsync(1);
        await _hub.Clients.All.SendAsync("KOTsUpdated", activeKots);
        return Ok(kot);
    }
}
