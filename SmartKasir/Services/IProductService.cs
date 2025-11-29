using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk operasi produk di client
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Get produk berdasarkan barcode
    /// </summary>
    Task<ProductDto?> GetByBarcodeAsync(string barcode);

    /// <summary>
    /// Search produk berdasarkan keyword
    /// </summary>
    Task<IEnumerable<ProductDto>> SearchAsync(string keyword);

    /// <summary>
    /// Get semua produk
    /// </summary>
    Task<IEnumerable<ProductDto>> GetAllAsync();

    /// <summary>
    /// Get produk dengan pagination
    /// </summary>
    Task<ApiPagedResult<ProductDto>> GetPagedAsync(int page = 1, int pageSize = 50);

    /// <summary>
    /// Create produk baru
    /// </summary>
    Task<ProductDto> CreateAsync(CreateProductRequest request);

    /// <summary>
    /// Update produk
    /// </summary>
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request);

    /// <summary>
    /// Delete produk
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Refresh product cache dari server
    /// </summary>
    Task RefreshCacheAsync();

    /// <summary>
    /// Create produk lokal (untuk offline/admin)
    /// </summary>
    Task<ProductDto> CreateLocalAsync(string barcode, string name, decimal price, int stockQty, string categoryName);

    /// <summary>
    /// Update produk lokal
    /// </summary>
    Task<ProductDto> UpdateLocalAsync(Guid id, string name, decimal price, int stockQty, bool isActive);

    /// <summary>
    /// Delete produk lokal
    /// </summary>
    Task DeleteLocalAsync(Guid id);
}
