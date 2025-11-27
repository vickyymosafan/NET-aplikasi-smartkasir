using SmartKasir.Application.DTOs;
using System.Printing;

namespace SmartKasir.Client.Services;

/// <summary>
/// Implementation dari IPrinterService
/// Handles ESC/POS thermal printer operations
/// </summary>
public class PrinterService : IPrinterService
{
    private string? _printerName;
    private bool _isPrinterAvailable;

    public event EventHandler<PrinterStatusEventArgs>? PrinterStatusChanged;

    public bool IsPrinterAvailable => _isPrinterAvailable;

    public string? PrinterName => _printerName;

    public PrinterService()
    {
        CheckPrinterAvailability();
    }

    public async Task<bool> PrintReceiptAsync(TransactionDto transaction)
    {
        if (!_isPrinterAvailable || string.IsNullOrEmpty(_printerName))
        {
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = "Printer not available"
            });
            return false;
        }

        try
        {
            var receipt = GenerateReceiptContent(transaction);
            await PrintToThermalPrinterAsync(receipt);
            return true;
        }
        catch (Exception ex)
        {
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = $"Print failed: {ex.Message}"
            });
            return false;
        }
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
                Message = _isPrinterAvailable ? $"Printer ready: {_printerName}" : "No printer available"
            });
        }
        catch
        {
            _isPrinterAvailable = false;
            OnPrinterStatusChanged(new PrinterStatusEventArgs
            {
                IsAvailable = false,
                Message = "Failed to check printer availability"
            });
        }
    }

    private string GenerateReceiptContent(TransactionDto transaction)
    {
        var receipt = new System.Text.StringBuilder();

        // Header
        receipt.AppendLine("================================");
        receipt.AppendLine("        SMARTKASIR RECEIPT");
        receipt.AppendLine("================================");
        receipt.AppendLine();

        // Invoice info
        receipt.AppendLine($"Invoice: {transaction.InvoiceNumber}");
        receipt.AppendLine($"Date: {transaction.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        receipt.AppendLine($"Cashier: {transaction.CashierName}");
        receipt.AppendLine();

        // Items
        receipt.AppendLine("ITEMS:");
        receipt.AppendLine("--------------------------------");
        foreach (var item in transaction.Items)
        {
            var line = $"{item.ProductName,-20} {item.Quantity,3}x {item.PriceAtMoment,10:C}";
            receipt.AppendLine(line);
            receipt.AppendLine($"  Subtotal: {item.Subtotal,30:C}");
        }

        receipt.AppendLine("--------------------------------");
        receipt.AppendLine();

        // Totals
        receipt.AppendLine($"Subtotal: {(transaction.TotalAmount - transaction.TaxAmount),25:C}");
        receipt.AppendLine($"Tax (10%): {transaction.TaxAmount,25:C}");
        receipt.AppendLine($"TOTAL: {transaction.TotalAmount,30:C}");
        receipt.AppendLine();

        // Payment method
        receipt.AppendLine($"Payment: {transaction.PaymentMethod}");
        receipt.AppendLine();

        // Footer
        receipt.AppendLine("================================");
        receipt.AppendLine("    Thank you for your purchase!");
        receipt.AppendLine("================================");
        receipt.AppendLine();

        return receipt.ToString();
    }

    private async Task PrintToThermalPrinterAsync(string content)
    {
        if (string.IsNullOrEmpty(_printerName))
        {
            throw new InvalidOperationException("No printer selected");
        }

        try
        {
            // Use Windows Print API
            var printQueue = new PrintQueue(new PrintServer(), _printerName);
            var printJob = printQueue.AddJob("SmartKasir Receipt");

            using (var stream = printJob.JobStream)
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to print: {ex.Message}", ex);
        }
    }

    private void OnPrinterStatusChanged(PrinterStatusEventArgs args)
    {
        PrinterStatusChanged?.Invoke(this, args);
    }
}
