using SmartKasir.Core.Entities;

namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Repository interface untuk User entity
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Mendapatkan user berdasarkan username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Mendapatkan semua user yang aktif
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    /// Mengecek apakah username sudah ada
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
}
