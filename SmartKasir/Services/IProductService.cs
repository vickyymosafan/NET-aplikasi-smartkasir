using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk operasi produk
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
    Task<PagedResult<ProductDto>> GetPagedAsync(int page = 1, int pageSize = 50);

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
}
