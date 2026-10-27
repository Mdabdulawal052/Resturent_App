using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class ShiftsController : ApiControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService) => _shiftService = shiftService;

    [HttpPost("open")]
    public async Task<ActionResult<CashShiftDto>> OpenShift([FromBody] OpenShiftRequest req, [FromQuery] long userId = 2)
    {
        return Ok(await _shiftService.OpenShiftAsync(req, userId));
    }

    [HttpPost("close")]
    public async Task<ActionResult<CashShiftDto>> CloseShift([FromBody] CloseShiftRequest req)
    {
        return Ok(await _shiftService.CloseShiftAsync(req));
    }
}
