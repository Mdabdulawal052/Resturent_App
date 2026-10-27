using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;

namespace Restaurant.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _db;

    public BookingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TableBookingDto>> GetBookingsAsync(long branchId, DateTime? date = null, string? timeSlot = null, string? status = null)
    {
        var query = _db.Bookings
            .Where(b => b.BranchId == branchId);

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(b => b.BookingDate.Date == targetDate);
        }

        if (!string.IsNullOrEmpty(timeSlot))
        {
            query = query.Where(b => b.TimeSlot == timeSlot);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(b => b.Status == status);
        }

        var list = await query
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .ToListAsync();

        return list.Select(MapToDto).ToList();
    }

    public async Task<TableBookingDto?> GetBookingByIdAsync(long id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        return booking == null ? null : MapToDto(booking);
    }

    public async Task<TableBookingDto> CreateBookingAsync(CreateBookingRequest req, long userId = 1)
    {
        var count = await _db.Bookings.CountAsync(b => b.BranchId == req.BranchId && b.BookingDate.Date == req.BookingDate.Date);
        var bookingNo = $"BKG-{req.BookingDate:yyyyMMdd}-{(count + 1):000}";

        var booking = new TableBooking
        {
            BranchId = req.BranchId,
            BookingNo = bookingNo,
            BookingType = string.IsNullOrEmpty(req.BookingType) ? "Table" : req.BookingType,
            TableId = req.TableId,
            TableNo = req.TableNo,
            SeatNumber = req.SeatNumber,
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            CustomerEmail = req.CustomerEmail,
            GuestCount = req.GuestCount > 0 ? req.GuestCount : (req.BookingType == "Chair" ? 1 : 2),
            BookingDate = req.BookingDate.Date,
            TimeSlot = string.IsNullOrEmpty(req.TimeSlot) ? "07:00 PM - 09:00 PM" : req.TimeSlot,
            Status = string.IsNullOrEmpty(req.Status) ? "Pending" : req.Status,
            SpecialRequests = req.SpecialRequests,
            DepositAmount = req.DepositAmount,
            IsDepositPaid = req.IsDepositPaid,
            CreatedByUserId = userId
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<TableBookingDto> UpdateBookingStatusAsync(long bookingId, string status)
    {
        var booking = await _db.Bookings.FindAsync(bookingId) ?? throw new KeyNotFoundException("Booking not found");
        booking.Status = status;

        if (status.Equals("Seated", StringComparison.OrdinalIgnoreCase))
        {
            var table = await _db.Tables.FindAsync(booking.TableId);
            if (table != null)
            {
                table.Status = Domain.Enums.TableStatus.Occupied;
                table.CurrentGuestCount = booking.GuestCount;
            }
        }

        await _db.SaveChangesAsync();
        return MapToDto(booking);
    }

    public async Task<bool> CancelBookingAsync(long bookingId)
    {
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking == null) return false;

        booking.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return true;
    }

    private static TableBookingDto MapToDto(TableBooking b) => new(
        b.Id,
        b.BranchId,
        b.BookingNo,
        b.BookingType,
        b.TableId,
        b.TableNo,
        b.SeatNumber,
        b.CustomerName,
        b.CustomerPhone,
        b.CustomerEmail,
        b.GuestCount,
        b.BookingDate,
        b.TimeSlot,
        b.Status,
        b.SpecialRequests,
        b.DepositAmount,
        b.IsDepositPaid,
        b.CreatedAtUtc
    );
}
