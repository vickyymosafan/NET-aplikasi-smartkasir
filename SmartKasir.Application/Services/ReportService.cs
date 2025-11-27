using SmartKasir.Application.DTOs;
using SmartKasir.Core.Interfaces;

namespace SmartKasir.Application.Services;

/// <summary>
/// Implementasi layanan laporan
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DailySalesReport> GetDailySalesAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1).AddTicks(-1);

        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startOfDay, endOfDay);
        var transactionList = transactions.ToList();

        var items = transactionList.Select(t => new DailySalesItem(
            t.InvoiceNumber,
            t.Cashier?.Username ?? "Unknown",
            t.TotalAmount,
            t.CreatedAt)).ToList();

        return new DailySalesReport(
            date,
            transactionList.Count,
            transactionList.Sum(t => t.TotalAmount),
            transactionList.Sum(t => t.TaxAmount),
            items);
    }

    public async Task<ProductSalesReport> GetProductSalesAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);
        var transactionList = transactions.ToList();

        // Agregasi per produk
        var productSales = transactionList
            .SelectMany(t => t.Items)
            .GroupBy(i => new { i.ProductId, ProductName = i.Product?.Name ?? "Unknown", CategoryName = i.Product?.Category?.Name ?? "Unknown" })
            .Select(g => new ProductSalesItem(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                g.Sum(i => i.Quantity),
                g.Sum(i => i.Subtotal)))
            .OrderByDescending(p => p.Revenue)
            .ToList();


        return new ProductSalesReport(
            startDate,
            endDate,
            productSales,
            productSales.Sum(p => p.Revenue),
            productSales.Sum(p => p.QuantitySold));
    }

    public async Task<byte[]> ExportToPdfAsync(ReportFilterParams filter)
    {
        var report = await GetProductSalesAsync(filter.StartDate, filter.EndDate);
        
        // Implementasi sederhana - dalam produksi gunakan library seperti iTextSharp atau QuestPDF
        // Untuk saat ini, return placeholder
        var content = GenerateReportContent(report);
        return System.Text.Encoding.UTF8.GetBytes(content);
    }

    public async Task<byte[]> ExportToExcelAsync(ReportFilterParams filter)
    {
        var report = await GetProductSalesAsync(filter.StartDate, filter.EndDate);
        
        // Implementasi sederhana - dalam produksi gunakan library seperti EPPlus atau ClosedXML
        // Untuk saat ini, return CSV format
        var csv = GenerateCsvContent(report);
        return System.Text.Encoding.UTF8.GetBytes(csv);
    }

    private static string GenerateReportContent(ProductSalesReport report)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Laporan Penjualan Produk");
        sb.AppendLine($"Periode: {report.StartDate:dd/MM/yyyy} - {report.EndDate:dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine($"Total Revenue: Rp {report.TotalRevenue:N0}");
        sb.AppendLine($"Total Quantity Sold: {report.TotalQuantitySold}");
        sb.AppendLine();
        sb.AppendLine("Detail per Produk:");
        sb.AppendLine("-------------------");

        foreach (var item in report.Items)
        {
            sb.AppendLine($"{item.ProductName} ({item.CategoryName})");
            sb.AppendLine($"  Qty: {item.QuantitySold}, Revenue: Rp {item.Revenue:N0}");
        }

        return sb.ToString();
    }

    private static string GenerateCsvContent(ProductSalesReport report)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ProductId,ProductName,CategoryName,QuantitySold,Revenue");

        foreach (var item in report.Items)
        {
            sb.AppendLine($"{item.ProductId},{item.ProductName},{item.CategoryName},{item.QuantitySold},{item.Revenue}");
        }

        return sb.ToString();
    }
}
