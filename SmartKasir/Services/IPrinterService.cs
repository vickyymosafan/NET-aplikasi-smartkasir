using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

/// <summary>
/// Service untuk operasi printer thermal
/// </summary>
public interface IPrinterService
{
    /// <summary>
    /// Print receipt untuk transaksi
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
