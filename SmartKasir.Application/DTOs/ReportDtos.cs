namespace SmartKasir.Application.DTOs;

/// <summary>
/// Laporan penjualan harian
/// </summary>
public class DailySalesReport
{
    public DateTime Date { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public List<DailySalesItem> Items { get; set; } = new();

    public DailySalesReport() { }
    public DailySalesReport(DateTime date, int totalTransactions, decimal totalRevenue, decimal totalTax, List<DailySalesItem> items)
    {
        Date = date;
        TotalTransactions = totalTransactions;
        TotalRevenue = totalRevenue;
        TotalTax = totalTax;
        Items = items;
    }
}

/// <summary>
/// Item dalam laporan harian
/// </summary>
public class DailySalesItem
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public DailySalesItem() { }
    public DailySalesItem(string invoiceNumber, string cashierName, decimal amount, DateTime createdAt)
    {
        InvoiceNumber = invoiceNumber;
        CashierName = cashierName;
        Amount = amount;
        CreatedAt = createdAt;
    }
}

/// <summary>
/// Laporan penjualan per produk
/// </summary>
public class ProductSalesReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ProductSalesItem> Items { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public int TotalQuantitySold { get; set; }

    public ProductSalesReport() { }
    public ProductSalesReport(DateTime startDate, DateTime endDate, List<ProductSalesItem> items, decimal totalRevenue, int totalQuantitySold)
    {
        StartDate = startDate;
        EndDate = endDate;
        Items = items;
        TotalRevenue = totalRevenue;
        TotalQuantitySold = totalQuantitySold;
    }
}

/// <summary>
/// Item dalam laporan per produk
/// </summary>
public class ProductSalesItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }

    public ProductSalesItem() { }
    public ProductSalesItem(Guid productId, string productName, string categoryName, int quantitySold, decimal revenue)
    {
        ProductId = productId;
        ProductName = productName;
        CategoryName = categoryName;
        QuantitySold = quantitySold;
        Revenue = revenue;
    }
}

/// <summary>
/// Filter untuk laporan
/// </summary>
public class ReportFilterParams
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
