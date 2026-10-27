using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Services.Interfaces;

namespace Restaurant.Api.Controllers;

public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpGet("sales-summary")]
    public async Task<ActionResult<SalesSummaryDto>> GetSalesSummary([FromQuery] long branchId = 1, [FromQuery] DateTime? date = null)
    {
        return Ok(await _reportService.GetSalesSummaryAsync(branchId, date ?? DateTime.UtcNow));
    }

    [HttpGet("vat")]
    public async Task<ActionResult<List<VatReportDto>>> GetVatReport([FromQuery] long branchId = 1, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _reportService.GetVatReportAsync(branchId, fromDate ?? DateTime.UtcNow.AddDays(-30), toDate ?? DateTime.UtcNow));
    }
}
