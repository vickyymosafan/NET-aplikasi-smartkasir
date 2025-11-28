using System.Text;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services.Printing;

/// <summary>
/// Builder untuk membuat receipt dengan ESC/POS formatting
/// Requirements: 5.4 - Struk include invoice, items, total, payment, date
/// </summary>
public class ReceiptBuilder
{
    private readonly List<byte> _buffer = new();
    private readonly Encoding _encoding = Encoding.GetEncoding(437); // PC437 for thermal printers
    private const int LineWidth = 42; // Standard 80mm thermal printer width

    public ReceiptBuilder()
    {
        // Initialize printer
        AddBytes(EscPosCommands.Initialize);
        AddBytes(EscPosCommands.CharsetPC437);
    }

    /// <summary>
    /// Build receipt dari TransactionDto
    /// Property 13: Konten Struk - include invoice, items, total, payment, date
    /// </summary>
    public byte[] BuildReceipt(TransactionDto transaction, string storeName = "SMARTKASIR")
    {
        // Header
        AddCenteredBold($"{storeName}");
        AddCenteredText("================================");
        AddLine();

        // Invoice info (Requirement 5.4)
        AddText($"Invoice : {transaction.InvoiceNumber}");
        AddText($"Tanggal : {transaction.CreatedAt:dd/MM/yyyy HH:mm}");
        AddText($"Kasir   : {transaction.CashierName}");
        AddSeparator();

        // Items (Requirement 5.4)
        foreach (var item in transaction.Items)
        {
            AddText(item.ProductName);
            AddRightAlignedText($"{item.Quantity} x {item.PriceAtMoment:N0} = {item.Subtotal:N0}");
        }

        AddSeparator();

        // Totals (Requirement 5.4)
        var subtotal = transaction.TotalAmount - transaction.TaxAmount;
        AddTwoColumnText("Subtotal", $"Rp {subtotal:N0}");
        AddTwoColumnText("Pajak (10%)", $"Rp {transaction.TaxAmount:N0}");
        AddBytes(EscPosCommands.BoldOn);
        AddTwoColumnText("TOTAL", $"Rp {transaction.TotalAmount:N0}");
        AddBytes(EscPosCommands.BoldOff);

        AddSeparator();

        // Payment method (Requirement 5.4)
        AddTwoColumnText("Pembayaran", transaction.PaymentMethod.ToString());

        AddLine();
        AddLine();

        // Footer
        AddCenteredText("Terima kasih atas kunjungan Anda!");
        AddCenteredText("================================");

        // Feed and cut
        AddBytes(EscPosCommands.FeedLines5);
        AddBytes(EscPosCommands.CutPaperPartial);

        return _buffer.ToArray();
    }

    /// <summary>
    /// Get receipt as plain text (for preview/logging)
    /// </summary>
    public string BuildReceiptText(TransactionDto transaction, string storeName = "SMARTKASIR")
    {
        var sb = new StringBuilder();

        sb.AppendLine(CenterText(storeName));
        sb.AppendLine("================================");
        sb.AppendLine();
        sb.AppendLine($"Invoice : {transaction.InvoiceNumber}");
        sb.AppendLine($"Tanggal : {transaction.CreatedAt:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Kasir   : {transaction.CashierName}");
        sb.AppendLine("--------------------------------");

        foreach (var item in transaction.Items)
        {
            sb.AppendLine(item.ProductName);
            sb.AppendLine($"  {item.Quantity} x {item.PriceAtMoment:N0} = Rp {item.Subtotal:N0}");
        }

        sb.AppendLine("--------------------------------");

        var subtotal = transaction.TotalAmount - transaction.TaxAmount;
        sb.AppendLine($"{"Subtotal",-20} Rp {subtotal,15:N0}");
        sb.AppendLine($"{"Pajak (10%)",-20} Rp {transaction.TaxAmount,15:N0}");
        sb.AppendLine($"{"TOTAL",-20} Rp {transaction.TotalAmount,15:N0}");
        sb.AppendLine("--------------------------------");
        sb.AppendLine($"{"Pembayaran",-20} {transaction.PaymentMethod,15}");
        sb.AppendLine();
        sb.AppendLine(CenterText("Terima kasih atas kunjungan Anda!"));
        sb.AppendLine("================================");

        return sb.ToString();
    }

    #region Helper Methods

    private void AddBytes(byte[] bytes)
    {
        _buffer.AddRange(bytes);
    }

    private void AddText(string text)
    {
        AddBytes(_encoding.GetBytes(text));
        AddBytes(EscPosCommands.LF);
    }

    private void AddLine()
    {
        AddBytes(EscPosCommands.LF);
    }

    private void AddCenteredText(string text)
    {
        AddBytes(EscPosCommands.AlignCenter);
        AddText(text);
        AddBytes(EscPosCommands.AlignLeft);
    }

    private void AddCenteredBold(string text)
    {
        AddBytes(EscPosCommands.AlignCenter);
        AddBytes(EscPosCommands.DoubleHeightWidthOn);
        AddText(text);
        AddBytes(EscPosCommands.NormalSize);
        AddBytes(EscPosCommands.AlignLeft);
    }

    private void AddRightAlignedText(string text)
    {
        var padding = new string(' ', Math.Max(0, LineWidth - text.Length - 2));
        AddText($"  {padding}{text}");
    }

    private void AddTwoColumnText(string left, string right)
    {
        var spaces = LineWidth - left.Length - right.Length;
        var padding = new string(' ', Math.Max(1, spaces));
        AddText($"{left}{padding}{right}");
    }

    private void AddSeparator()
    {
        AddText(new string('-', LineWidth));
    }

    private static string CenterText(string text)
    {
        var padding = (LineWidth - text.Length) / 2;
        return new string(' ', Math.Max(0, padding)) + text;
    }

    #endregion
}
