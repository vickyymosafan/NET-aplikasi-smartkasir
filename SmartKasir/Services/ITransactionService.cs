using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk operasi transaksi di client
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Process penjualan dan simpan transaksi
    /// </summary>
    Task<TransactionResult> ProcessSaleAsync(CreateTransactionRequest request);

    /// <summary>
    /// Get transaksi berdasarkan ID
    /// </summary>
    Task<TransactionDto?> GetTransactionAsync(Guid id);

    /// <summary>
    /// Get semua transaksi dengan filter
    /// </summary>
    Task<ApiPagedResult<TransactionDto>> GetTransactionsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Generate invoice number unik
    /// </summary>
    Task<string> GenerateInvoiceNumberAsync();

    /// <summary>
    /// Get pending transactions (belum sync)
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync();
}
