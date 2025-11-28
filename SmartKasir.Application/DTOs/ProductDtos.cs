namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data produk
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Request untuk membuat produk baru
/// </summary>
public class CreateProductRequest
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQty { get; set; }
    public int CategoryId { get; set; }
}

/// <summary>
/// Request untuk update produk
/// </summary>
public class UpdateProductRequest
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? StockQty { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
}
