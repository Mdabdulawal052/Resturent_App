using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpGet("methods")]
    public async Task<ActionResult<List<PaymentMethodDto>>> GetMethods([FromQuery] long restaurantId = 1)
    {
        return Ok(await _paymentService.GetPaymentMethodsAsync(restaurantId));
    }
}
