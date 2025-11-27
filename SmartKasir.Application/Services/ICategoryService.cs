using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan kategori
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Mendapatkan semua kategori
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllAsync();

    /// <summary>
    /// Mendapatkan kategori berdasarkan ID
    /// </summary>
    Task<CategoryDto?> GetByIdAsync(int id);

    /// <summary>
    /// Membuat kategori baru
    /// </summary>
    Task<OperationResult<CategoryDto>> CreateAsync(CreateCategoryRequest request);

    /// <summary>
    /// Memperbarui kategori
    /// </summary>
    Task<OperationResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request);

    /// <summary>
    /// Menghapus kategori
    /// </summary>
    Task<OperationResult> DeleteAsync(int id);
}
