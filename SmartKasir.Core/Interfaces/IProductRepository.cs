using SmartKasir.Core.Entities;

namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Repository interface untuk Product entity
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Mendapatkan produk berdasarkan barcode
    /// </summary>
    Task<Product?> GetByBarcodeAsync(string barcode);

    /// <summary>
    /// Mencari produk berdasarkan keyword (nama atau barcode)
    /// </summary>
    Task<IEnumerable<Product>> SearchAsync(string keyword);

    /// <summary>
    /// Mendapatkan semua produk yang aktif
    /// </summary>
    Task<IEnumerable<Product>> GetActiveProductsAsync();

    /// <summary>
    /// Mendapatkan produk berdasarkan kategori
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);

    /// <summary>
    /// Mengecek apakah barcode sudah ada
    /// </summary>
    Task<bool> BarcodeExistsAsync(string barcode);

    /// <summary>
    /// Mengecek apakah barcode sudah ada (kecuali untuk product ID tertentu)
    /// </summary>
    Task<bool> BarcodeExistsAsync(string barcode, Guid excludeProductId);
}
