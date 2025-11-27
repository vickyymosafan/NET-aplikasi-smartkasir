using SmartKasir.Core.Entities;

namespace SmartKasir.Core.Interfaces;

/// <summary>
/// Repository interface untuk Transaction entity
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Mendapatkan transaksi berdasarkan invoice number
    /// </summary>
    Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber);

    /// <summary>
    /// Mendapatkan transaksi berdasarkan kasir
    /// </summary>
    Task<IEnumerable<Transaction>> GetByCashierAsync(Guid cashierId);

    /// <summary>
    /// Mendapatkan transaksi dalam rentang tanggal
    /// </summary>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Mengecek apakah invoice number sudah ada
    /// </summary>
    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber);

    /// <summary>
    /// Mendapatkan nomor urut untuk invoice number hari ini
    /// </summary>
    Task<int> GetTodaySequenceNumberAsync();
}
