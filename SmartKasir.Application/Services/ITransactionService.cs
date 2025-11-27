using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Services;

/// <summary>
/// Interface untuk layanan transaksi
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Memproses transaksi penjualan
    /// </summary>
    Task<TransactionResult> ProcessSaleAsync(Guid cashierId, CreateTransactionRequest request);

    /// <summary>
    /// Mendapatkan transaksi berdasarkan ID
    /// </summary>
    Task<TransactionDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Mendapatkan transaksi berdasarkan invoice number
    /// </summary>
    Task<TransactionDto?> GetByInvoiceNumberAsync(string invoiceNumber);

    /// <summary>
    /// Mendapatkan semua transaksi dengan filter
    /// </summary>
    Task<PagedResult<TransactionDto>> GetAllAsync(TransactionFilterParams filter);

    /// <summary>
    /// Generate invoice number baru
    /// </summary>
    Task<string> GenerateInvoiceNumberAsync();
}
