namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Generic repository interface untuk semua entities
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Mendapatkan entity berdasarkan ID
    /// </summary>
    Task<T?> GetByIdAsync(object id);

    /// <summary>
    /// Mendapatkan semua entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Menambahkan entity baru
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Memperbarui entity yang sudah ada
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Menghapus entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Mengecek apakah entity dengan ID tertentu ada
    /// </summary>
    Task<bool> ExistsAsync(object id);
}
