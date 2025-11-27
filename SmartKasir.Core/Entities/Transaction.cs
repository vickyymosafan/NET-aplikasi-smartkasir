using SmartKasir.Core.Enums;

namespace SmartKasir.Core.Entities;

/// <summary>
/// Entitas transaksi penjualan
/// </summary>
public class Transaction
{
    /// <summary>
    /// Identifier unik transaksi
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Nomor invoice dengan format INV-YYYYMMDD-XXXX
    /// </summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Foreign key ke kasir yang melakukan transaksi
    /// </summary>
    public Guid CashierId { get; private set; }

    /// <summary>
    /// Total jumlah uang transaksi
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Jumlah pajak
    /// </summary>
    public decimal TaxAmount { get; private set; }

    /// <summary>
    /// Metode pembayaran yang digunakan
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; }

    /// <summary>
    /// Waktu transaksi dibuat
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Navigation property ke kasir
    /// </summary>
    public User? Cashier { get; private set; }

    /// <summary>
    /// Koleksi item dalam transaksi
    /// </summary>
    public ICollection<TransactionItem> Items { get; private set; } = new List<TransactionItem>();

    /// <summary>
    /// Constructor untuk membuat transaksi baru
    /// </summary>
    public Transaction(
        string invoiceNumber,
        Guid cashierId,
        decimal totalAmount,
        decimal taxAmount,
        PaymentMethod paymentMethod)
    {
        Id = Guid.NewGuid();
        InvoiceNumber = invoiceNumber;
        CashierId = cashierId;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        PaymentMethod = paymentMethod;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor untuk EF Core
    /// </summary>
    private Transaction() { }

    /// <summary>
    /// Menambahkan item ke transaksi
    /// </summary>
    public void AddItem(TransactionItem item)
    {
        Items.Add(item);
    }

    /// <summary>
    /// Mengecek apakah invoice number valid (format INV-YYYYMMDD-XXXX)
    /// </summary>
    public static bool IsValidInvoiceNumber(string invoiceNumber)
    {
        if (string.IsNullOrEmpty(invoiceNumber))
            return false;

        // Format: INV-YYYYMMDD-XXXX
        var parts = invoiceNumber.Split('-');
        if (parts.Length != 3)
            return false;

        if (parts[0] != "INV")
            return false;

        if (!DateTime.TryParseExact(parts[1], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
            return false;

        if (parts[2].Length != 4 || !parts[2].All(char.IsDigit))
            return false;

        return true;
    }

    /// <summary>
    /// Menghasilkan invoice number dengan format INV-YYYYMMDD-XXXX
    /// </summary>
    public static string GenerateInvoiceNumber(int sequenceNumber)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = sequenceNumber.ToString("D4");
        return $"INV-{date}-{sequence}";
    }
}
