using Restaurant.Domain.Common;

namespace Restaurant.Domain.Entities;

public class TableBooking : AuditableEntity
{
    public long BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public string BookingType { get; set; } = "Table"; // "Table" | "Chair"
    public long TableId { get; set; }
    public RestaurantTable? Table { get; set; }
    public string TableNo { get; set; } = string.Empty;
    public int? SeatNumber { get; set; } // 1, 2, 3... when BookingType == "Chair"
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int GuestCount { get; set; } = 2;
    public DateTime BookingDate { get; set; }
    public string TimeSlot { get; set; } = "07:00 PM - 09:00 PM";
    public TimeSpan StartTime { get; set; } = new TimeSpan(19, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(21, 0, 0);
    public string Status { get; set; } = "Confirmed"; // Confirmed, Seated, Cancelled, Completed
    public string? SpecialRequests { get; set; }
    public decimal DepositAmount { get; set; } = 0;
    public bool IsDepositPaid { get; set; } = false;
}
