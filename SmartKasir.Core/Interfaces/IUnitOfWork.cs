namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Unit of Work pattern untuk mengelola transaksi database
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Repository untuk User
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Repository untuk Product
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Repository untuk Category
    /// </summary>
    ICategoryRepository Categories { get; }

    /// <summary>
    /// Repository untuk Transaction
    /// </summary>
    ITransactionRepository Transactions { get; }

    /// <summary>
    /// Menyimpan semua perubahan ke database
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Memulai transaksi database
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Melakukan commit transaksi
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Melakukan rollback transaksi
    /// </summary>
    Task RollbackAsync();
}
