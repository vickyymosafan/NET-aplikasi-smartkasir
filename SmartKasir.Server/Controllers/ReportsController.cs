using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKasir.Application.DTOs;
using SmartKasir.Application.Services;

namespace SmartKasir.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Mendapatkan laporan penjualan harian
    /// </summary>
    [HttpGet("daily")]
    public async Task<ActionResult<DailySalesReport>> GetDailySales([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.Today;
        var report = await _reportService.GetDailySalesAsync(reportDate);
        return Ok(report);
    }

    /// <summary>
    /// Mendapatkan laporan penjualan per produk
    /// </summary>
    [HttpGet("products")]
    public async Task<ActionResult<ProductSalesReport>> GetProductSales([FromQuery] ReportFilterParams filter)
    {
        var report = await _reportService.GetProductSalesAsync(filter.StartDate, filter.EndDate);
        return Ok(report);
    }

    /// <summary>
    /// Export laporan ke PDF
    /// </summary>
    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportToPdf([FromQuery] ReportFilterParams filter)
    {
        var pdfBytes = await _reportService.ExportToPdfAsync(filter);
        return File(pdfBytes, "application/pdf", $"report_{filter.StartDate:yyyyMMdd}_{filter.EndDate:yyyyMMdd}.pdf");
    }


    /// <summary>
    /// Export laporan ke Excel
    /// </summary>
    [HttpGet("export/excel")]
    public async Task<IActionResult> ExportToExcel([FromQuery] ReportFilterParams filter)
    {
        var excelBytes = await _reportService.ExportToExcelAsync(filter);
        return File(excelBytes, "text/csv", $"report_{filter.StartDate:yyyyMMdd}_{filter.EndDate:yyyyMMdd}.csv");
    }
}
