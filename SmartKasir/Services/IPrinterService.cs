using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services.Printing;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk operasi printer thermal
/// Requirements: 5.1, 5.2, 5.3, 5.4
/// </summary>
public interface IPrinterService
{
    /// <summary>
    /// Print receipt untuk transaksi (Requirement 5.1, 5.2)
    /// </summary>
    Task<bool> PrintReceiptAsync(TransactionDto transaction);

    /// <summary>
    /// Check apakah printer tersedia
    /// </summary>
    bool IsPrinterAvailable { get; }

    /// <summary>
    /// Get nama printer yang digunakan
    /// </summary>
    string? PrinterName { get; }

    /// <summary>
    /// Set printer yang akan digunakan
    /// </summary>
    Task SetPrinterAsync(string printerName);

    /// <summary>
    /// Get daftar printer yang tersedia
    /// </summary>
    Task<IEnumerable<string>> GetAvailablePrintersAsync();

    /// <summary>
    /// Get receipt preview as text
    /// </summary>
    string GetReceiptPreview(TransactionDto transaction);

    /// <summary>
    /// Retry all failed print jobs (Requirement 5.3)
    /// </summary>
    int RetryFailedPrints();

    /// <summary>
    /// Get count of pending prints
    /// </summary>
    int GetPendingPrintCount();

    /// <summary>
    /// Get print queue for retry management (Requirement 5.3)
    /// </summary>
    PrintQueue Queue { get; }

    /// <summary>
    /// Event ketika printer status berubah
    /// </summary>
    event EventHandler<PrinterStatusEventArgs>? PrinterStatusChanged;
}

/// <summary>
/// Event args untuk printer status change
/// </summary>
public class PrinterStatusEventArgs : EventArgs
{
    public bool IsAvailable { get; set; }
    public string? Message { get; set; }
}
