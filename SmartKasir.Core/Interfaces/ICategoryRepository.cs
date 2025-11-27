using SmartKasir.Core.Entities;

namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Repository interface untuk Category entity
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Mendapatkan kategori berdasarkan nama
    /// </summary>
    Task<Category?> GetByNameAsync(string name);

    /// <summary>
    /// Mengecek apakah kategori memiliki produk
    /// </summary>
    Task<bool> HasProductsAsync(int categoryId);

    /// <summary>
    /// Mengecek apakah nama kategori sudah ada
    /// </summary>
    Task<bool> NameExistsAsync(string name);

    /// <summary>
    /// Mengecek apakah nama kategori sudah ada (kecuali untuk category ID tertentu)
    /// </summary>
    Task<bool> NameExistsAsync(string name, int excludeCategoryId);
}
