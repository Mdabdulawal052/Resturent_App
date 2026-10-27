using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Entities;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IAppDbContext _db;

    public ShiftService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<CashShiftDto> OpenShiftAsync(OpenShiftRequest req, long userId)
    {
        var shift = new CashShift
        {
            BranchId = req.BranchId,
            TerminalId = req.TerminalId,
            UserId = userId,
            OpeningCash = req.OpeningCash,
            OpenedAtUtc = DateTime.UtcNow
        };

        _db.CashShifts.Add(shift);
        await _db.SaveChangesAsync();

        return new CashShiftDto(
            shift.Id,
            shift.BranchId,
            shift.UserId,
            "Active Cashier",
            shift.OpenedAtUtc,
            shift.OpeningCash,
            null,
            null,
            null,
            null,
            null,
            0, 0, 0, 0, 0, 0, 0, 0,
            new List<CashMovementDto>()
        );
    }

    public async Task<CashShiftDto> CloseShiftAsync(CloseShiftRequest req)
    {
        var shift = await _db.CashShifts
            .Include(s => s.Movements)
            .FirstOrDefaultAsync(s => s.Id == req.ShiftId)
            ?? throw new KeyNotFoundException("Shift not found");

        var cashSales = await _db.Payments
            .Include(p => p.PaymentMethod)
            .Where(p => p.CreatedByUserId == shift.UserId && p.PaidAtUtc >= shift.OpenedAtUtc && p.PaymentMethod != null && p.PaymentMethod.IsCash)
            .SumAsync(p => p.Amount);

        var netMovements = shift.Movements.Sum(m => m.MovementType == CashMovementType.CashIn ? m.Amount : -m.Amount);
        var expectedCash = shift.OpeningCash + cashSales + netMovements;
        var variance = req.ClosingCash - expectedCash;

        shift.ClosedAtUtc = DateTime.UtcNow;
        shift.ClosingCash = req.ClosingCash;
        shift.ExpectedCash = expectedCash;
        shift.Variance = variance;
        shift.Notes = req.Notes;

        shift.Count1000 = req.Count1000;
        shift.Count500 = req.Count500;
        shift.Count200 = req.Count200;
        shift.Count100 = req.Count100;
        shift.Count50 = req.Count50;
        shift.Count20 = req.Count20;
        shift.Count10 = req.Count10;
        shift.DeclaredCash = req.DeclaredCash > 0 ? req.DeclaredCash : req.ClosingCash;

        await _db.SaveChangesAsync();

        return new CashShiftDto(
            shift.Id,
            shift.BranchId,
            shift.UserId,
            "Cashier",
            shift.OpenedAtUtc,
            shift.OpeningCash,
            shift.ClosedAtUtc,
            shift.ClosingCash,
            shift.ExpectedCash,
            shift.Variance,
            shift.Notes,
            shift.Count1000,
            shift.Count500,
            shift.Count200,
            shift.Count100,
            shift.Count50,
            shift.Count20,
            shift.Count10,
            shift.DeclaredCash,
            shift.Movements.Select(m => new CashMovementDto(m.Id, m.MovementType, m.Amount, m.Reason, m.CreatedAtUtc)).ToList()
        );
    }
}
