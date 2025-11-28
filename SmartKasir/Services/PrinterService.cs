using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services.Printing;
using System.Printing;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari IPrinterService
/// Handles ESC/POS thermal printer operations
/// Requirements: 5.1, 5.2, 5.3
/// </summary>
public class PrinterService : IPrinterService
{
    private string? _printerName;
    private bool _isPrinterAvailable;
    private readonly ReceiptBuilder _receiptBuilder;
    private readonly Printing.PrintQueue _printQueue;

    public event EventHandler<PrinterStatusEventArgs>? PrinterStatusChanged;

    public bool IsPrinterAvailable => _isPrinterAvailable;
    public string? PrinterName => _printerName;

    /// <summary>
    /// Get print queue for retry management (Requirement 5.3)
    /// </summary>
    public Printing.PrintQueue Queue => _printQueue;

    public PrinterService()
    {
        _receiptBuilder = new ReceiptBuilder();
        _printQueue = new Printing.PrintQueue(maxRetries: 3, retryDelaySeconds: 5);
        
        // Subscribe to queue events
        _printQueue.JobFailed += OnPrintJobFailed;
        _printQueue.JobCompleted += OnPrintJobCompleted;
        
        CheckPrinterAvailability();
        
        // Start queue processing
        _printQueue.StartProcessing(PrintReceiptInternalAsync);
    }

    /// <summary>
    /// Print receipt - adds to queue for processing (Requirement 5.1)
    /// </summary>
    public async Task<bool> PrintReceiptAsync(TransactionDto transaction)
    {
        if (!_isPrinterAvailable || string.IsNullOrEmpty(_printerName))
        {
            // Store for later retry (Requirement 5.3)
            _printQueue.Enqueue(transaction);
            
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = "Printer tidak tersedia. Struk disimpan untuk dicetak ulang."
            });
            return false;
        }

        try
        {
            return await PrintReceiptInternalAsync(transaction);
        }
        catch (Exception ex)
        {
            // Store for retry on failure (Requirement 5.3)
            _printQueue.Enqueue(transaction);
            
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = $"Gagal mencetak: {ex.Message}. Struk disimpan untuk dicetak ulang."
            });
            return false;
        }
    }

    /// <summary>
    /// Internal print method using ESC/POS commands (Requirement 5.2)
    /// </summary>
    private async Task<bool> PrintReceiptInternalAsync(TransactionDto transaction)
    {
        if (string.IsNullOrEmpty(_printerName))
        {
            throw new InvalidOperationException("No printer selected");
        }

        try
        {
            // Generate ESC/POS formatted receipt
            var receiptBytes = _receiptBuilder.BuildReceipt(transaction);
            
            // Send to thermal printer
            await PrintToThermalPrinterAsync(receiptBytes);
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to print: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get receipt preview as text
    /// </summary>
    public string GetReceiptPreview(TransactionDto transaction)
    {
        return _receiptBuilder.BuildReceiptText(transaction);
    }

    public async Task SetPrinterAsync(string printerName)
    {
        _printerName = printerName;
        CheckPrinterAvailability();
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<string>> GetAvailablePrintersAsync()
    {
        var printers = new List<string>();

        try
        {
            var printServer = new PrintServer();
            var printQueues = printServer.GetPrintQueues();

            foreach (var queue in printQueues)
            {
                printers.Add(queue.Name);
            }
        }
        catch
        {
            // If PrintServer fails, return empty list
        }

        return await Task.FromResult(printers);
    }

    /// <summary>
    /// Retry all failed print jobs (Requirement 5.3)
    /// </summary>
    public int RetryFailedPrints()
    {
        return _printQueue.RetryAllFailed();
    }

    /// <summary>
    /// Get count of pending prints
    /// </summary>
    public int GetPendingPrintCount()
    {
        return _printQueue.PendingCount + _printQueue.FailedCount;
    }

    private void CheckPrinterAvailability()
    {
        try
        {
            var printServer = new PrintServer();
            var printQueues = printServer.GetPrintQueues();

            if (string.IsNullOrEmpty(_printerName))
            {
                // Try to find default thermal printer
                var thermalPrinter = printQueues.FirstOrDefault(p =>
                    p.Name.Contains("thermal", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("pos", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains("receipt", StringComparison.OrdinalIgnoreCase));

                if (thermalPrinter != null)
                {
                    _printerName = thermalPrinter.Name;
                    _isPrinterAvailable = true;
                }
                else
                {
                    _isPrinterAvailable = false;
                }
            }
            else
            {
                _isPrinterAvailable = printQueues.Any(p => p.Name == _printerName);
            }

            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = _isPrinterAvailable,
                Message = _isPrinterAvailable ? $"Printer siap: {_printerName}" : "Tidak ada printer tersedia"
            });
        }
        catch
        {
            _isPrinterAvailable = false;
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = "Gagal memeriksa ketersediaan printer"
            });
        }
    }

    /// <summary>
    /// Send ESC/POS bytes to thermal printer (Requirement 5.2)
    /// </summary>
    private async Task PrintToThermalPrinterAsync(byte[] receiptBytes)
    {
        if (string.IsNullOrEmpty(_printerName))
        {
            throw new InvalidOperationException("No printer selected");
        }

        try
        {
            // Use Windows Print API with raw bytes
            var printQueue = new System.Printing.PrintQueue(new PrintServer(), _printerName);
            var printJob = printQueue.AddJob("SmartKasir Receipt");

            using (var stream = printJob.JobStream)
            {
                stream.Write(receiptBytes, 0, receiptBytes.Length);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to print: {ex.Message}", ex);
        }
    }

    private void OnPrintJobFailed(object? sender, PrintJobEventArgs e)
    {
        OnPrinterStatusChanged(new PrinterStatusEventArgs
        {
            IsAvailable = _isPrinterAvailable,
            Message = $"Gagal mencetak struk {e.Job.Transaction.InvoiceNumber}: {e.Job.LastError}"
        });
    }

    private void OnPrintJobCompleted(object? sender, PrintJobEventArgs e)
    {
        OnPrinterStatusChanged(new PrinterStatusEventArgs
        {
            IsAvailable = true,
            Message = $"Struk {e.Job.Transaction.InvoiceNumber} berhasil dicetak"
        });
    }

    private void OnPrinterStatusChanged(PrinterStatusEventArgs args)
    {
        PrinterStatusChanged?.Invoke(this, args);
    }
}
