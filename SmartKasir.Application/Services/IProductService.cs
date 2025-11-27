using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan produk
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Mendapatkan produk berdasarkan barcode
    /// </summary>
    Task<ProductDto?> GetByBarcodeAsync(string barcode);

    /// <summary>
    /// Mencari produk berdasarkan keyword
    /// </summary>
    Task<IEnumerable<ProductDto>> SearchAsync(string keyword);

    /// <summary>
    /// Mendapatkan semua produk aktif
    /// </summary>
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();

    /// <summary>
    /// Mendapatkan semua produk dengan pagination
    /// </summary>
    Task<PagedResult<ProductDto>> GetAllAsync(PaginationParams pagination);

    /// <summary>
    /// Mendapatkan produk berdasarkan ID
    /// </summary>
    Task<ProductDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Membuat produk baru
    /// </summary>
    Task<OperationResult<ProductDto>> CreateAsync(CreateProductRequest request);

    /// <summary>
    /// Memperbarui produk
    /// </summary>
    Task<OperationResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request);

    /// <summary>
    /// Menonaktifkan produk (soft delete)
    /// </summary>
    Task<OperationResult> DeactivateAsync(Guid id);

    /// <summary>
    /// Mengaktifkan produk
    /// </summary>
    Task<OperationResult> ActivateAsync(Guid id);
}
