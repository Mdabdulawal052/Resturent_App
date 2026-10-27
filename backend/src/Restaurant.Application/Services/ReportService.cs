using Microsoft.EntityFrameworkCore;
using Restaurant.Application.Common.Interfaces;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;
using Restaurant.Domain.Enums;

namespace Restaurant.Application.Services;

public class ReportService : IReportService
{
    private readonly IAppDbContext _db;

    public ReportService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<SalesSummaryDto> GetSalesSummaryAsync(long branchId, DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var orders = await _db.Orders
            .Include(o => o.Payments).ThenInclude(p => p.PaymentMethod)
            .Where(o => o.BranchId == branchId && o.OrderedAtUtc >= start && o.OrderedAtUtc < end && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var payments = orders.SelectMany(o => o.Payments).Where(p => p.Status == PaymentStatus.Paid).ToList();

        var cash = payments.Where(p => p.PaymentMethod != null && p.PaymentMethod.IsCash).Sum(p => p.Amount);
        var digital = payments.Where(p => p.PaymentMethod != null && !p.PaymentMethod.IsCash).Sum(p => p.Amount);

        return new SalesSummaryDto(
            Date: start,
            TotalOrders: orders.Count,
            GrossSales: orders.Sum(o => o.Subtotal),
            NetSales: orders.Sum(o => o.GrandTotal),
            TotalTax: orders.Sum(o => o.TaxAmount),
            TotalDiscount: orders.Sum(o => o.DiscountAmount),
            TotalServiceCharge: orders.Sum(o => o.ServiceChargeAmount),
            CashPayments: cash,
            DigitalPayments: digital
        );
    }

    public async Task<List<VatReportDto>> GetVatReportAsync(long branchId, DateTime fromDate, DateTime toDate)
    {
        var orders = await _db.Orders
            .Where(o => o.BranchId == branchId && o.OrderedAtUtc >= fromDate && o.OrderedAtUtc <= toDate && o.Status == OrderStatus.Paid)
            .ToListAsync();

        var totalTaxable = orders.Sum(o => o.Subtotal - o.DiscountAmount + o.ServiceChargeAmount);
        var totalVat = orders.Sum(o => o.TaxAmount);

        return new List<VatReportDto>
        {
            new VatReportDto("VAT-Standard", 5.0m, totalTaxable, totalVat)
        };
    }
}
