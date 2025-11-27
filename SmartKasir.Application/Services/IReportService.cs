using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan laporan
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Mendapatkan laporan penjualan harian
    /// </summary>
    Task<DailySalesReport> GetDailySalesAsync(DateTime date);

    /// <summary>
    /// Mendapatkan laporan penjualan per produk
    /// </summary>
    Task<ProductSalesReport> GetProductSalesAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Export laporan ke PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(ReportFilterParams filter);

    /// <summary>
    /// Export laporan ke Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(ReportFilterParams filter);
}
