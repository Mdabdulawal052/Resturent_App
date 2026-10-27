using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}
