using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class BookingsController : ApiControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<TableBookingDto>>> GetBookings(
        [FromQuery] long branchId = 1,
        [FromQuery] DateTime? date = null,
        [FromQuery] string? timeSlot = null,
        [FromQuery] string? status = null)
    {
        var bookings = await _bookingService.GetBookingsAsync(branchId, date, timeSlot, status);
        return Ok(bookings);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TableBookingDto>> GetById(long id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound(new { message = $"Booking {id} not found" });
        return Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<TableBookingDto>> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return BadRequest(new { message = "Customer name and phone number are required." });
        }

        var created = await _bookingService.CreateBookingAsync(request, 1);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<TableBookingDto>> UpdateStatus(long id, [FromBody] UpdateBookingStatusRequest request)
    {
        var updated = await _bookingService.UpdateBookingStatusAsync(id, request.Status);
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult> CancelBooking(long id)
    {
        var success = await _bookingService.CancelBookingAsync(id);
        if (!success) return NotFound(new { message = $"Booking {id} not found" });
        return Ok(new { message = "Booking cancelled successfully" });
    }
}
