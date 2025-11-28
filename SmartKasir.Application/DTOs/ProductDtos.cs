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

    public ProductDto() { }
    public ProductDto(Guid id, string barcode, string name, decimal price, int stockQty, int categoryId, string categoryName, bool isActive)
    {
        Id = id;
        Barcode = barcode;
        Name = name;
        Price = price;
        StockQty = stockQty;
        CategoryId = categoryId;
        CategoryName = categoryName;
        IsActive = isActive;
    }
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

    public CreateProductRequest() { }
    public CreateProductRequest(string barcode, string name, decimal price, int stockQty, int categoryId)
    {
        Barcode = barcode;
        Name = name;
        Price = price;
        StockQty = stockQty;
        CategoryId = categoryId;
    }
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

    public UpdateProductRequest() { }
    public UpdateProductRequest(string? name, decimal? price, int? stockQty, int? categoryId, bool? isActive)
    {
        Name = name;
        Price = price;
        StockQty = stockQty;
        CategoryId = categoryId;
        IsActive = isActive;
    }
}
