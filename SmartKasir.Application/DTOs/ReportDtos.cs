namespace SmartKasir.Application.DTOs;

/// <summary>
/// Laporan penjualan harian
/// </summary>
public record DailySalesReport(
    DateTime Date,
    int TotalTransactions,
    decimal TotalRevenue,
    decimal TotalTax,
    List<DailySalesItem> Items);

/// <summary>
/// Item dalam laporan harian
/// </summary>
public record DailySalesItem(
    string InvoiceNumber,
    string CashierName,
    decimal Amount,
    DateTime CreatedAt);

/// <summary>
/// Laporan penjualan per produk
/// </summary>
public record ProductSalesReport(
    DateTime StartDate,
    DateTime EndDate,
    List<ProductSalesItem> Items,
    decimal TotalRevenue,
    int TotalQuantitySold);

/// <summary>
/// Item dalam laporan per produk
/// </summary>
public record ProductSalesItem(
    Guid ProductId,
    string ProductName,
    string CategoryName,
    int QuantitySold,
    decimal Revenue);

/// <summary>
/// Filter untuk laporan
/// </summary>
public record ReportFilterParams(
    DateTime StartDate,
    DateTime EndDate);
