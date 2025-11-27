namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data produk
/// </summary>
public record ProductDto(
    Guid Id,
    string Barcode,
    string Name,
    decimal Price,
    int StockQty,
    int CategoryId,
    string CategoryName,
    bool IsActive);

/// <summary>
/// Request untuk membuat produk baru
/// </summary>
public record CreateProductRequest(
    string Barcode,
    string Name,
    decimal Price,
    int StockQty,
    int CategoryId);

/// <summary>
/// Request untuk update produk
/// </summary>
public record UpdateProductRequest(
    string? Name,
    decimal? Price,
    int? StockQty,
    int? CategoryId,
    bool? IsActive);
