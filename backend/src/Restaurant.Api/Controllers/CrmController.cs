using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class CrmController : ApiControllerBase
{
    private readonly ICrmService _crmService;

    public CrmController(ICrmService crmService) => _crmService = crmService;

    [HttpGet("customers")]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers([FromQuery] long restaurantId = 1, [FromQuery] string? search = null)
    {
        return Ok(await _crmService.GetCustomersAsync(restaurantId, search));
    }
}
