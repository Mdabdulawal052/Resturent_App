using Restaurant.Application.DTOs;

namespace Restaurant.Application.Services.Interfaces;

public interface IReportService
{
    Task<SalesSummaryDto> GetSalesSummaryAsync(long branchId, DateTime date);
    Task<List<VatReportDto>> GetVatReportAsync(long branchId, DateTime fromDate, DateTime toDate);
}
