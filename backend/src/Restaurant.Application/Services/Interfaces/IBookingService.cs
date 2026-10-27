using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IBookingService
{
    Task<List<TableBookingDto>> GetBookingsAsync(long branchId, DateTime? date = null, string? timeSlot = null, string? status = null);
    Task<TableBookingDto?> GetBookingByIdAsync(long id);
    Task<TableBookingDto> CreateBookingAsync(CreateBookingRequest req, long userId = 1);
    Task<TableBookingDto> UpdateBookingStatusAsync(long bookingId, string status);
    Task<bool> CancelBookingAsync(long bookingId);
}
