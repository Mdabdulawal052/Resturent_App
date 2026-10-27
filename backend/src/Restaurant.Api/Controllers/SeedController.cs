using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Services;

namespace Restaurant.Api.Controllers;

public class SeedController : ApiControllerBase
{
    private readonly SeedDataService _seedService;

    public SeedController(SeedDataService seedService) => _seedService = seedService;

    [HttpPost]
    public async Task<ActionResult> Seed()
    {
        await _seedService.SeedAsync();
        return Ok(new { message = "Bangladesh Restaurant dataset seeded successfully." });
    }
}
