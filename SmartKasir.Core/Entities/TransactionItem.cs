namespace SmartKasir.Core.Entities;

/// <summary>
/// Entitas item dalam transaksi penjualan
/// </summary>
public class TransactionItem
{
    /// <summary>
    /// Identifier unik item transaksi
    /// </summary>
    public long Id { get; private set; }

    /// <summary>
    /// Foreign key ke transaksi
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Foreign key ke produk
    /// </summary>
    public Guid ProductId { get; private set; }

    /// <summary>
    /// Jumlah produk yang dibeli
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Harga produk saat transaksi (snapshot untuk mencegah perubahan harga historis)
    /// </summary>
    public decimal PriceAtMoment { get; private set; }

    /// <summary>
    /// Subtotal item (Quantity × PriceAtMoment)
    /// </summary>
    public decimal Subtotal { get; private set; }

    /// <summary>
    /// Navigation property ke transaksi
    /// </summary>
    public Transaction? Transaction { get; private set; }

    /// <summary>
    /// Navigation property ke produk
    /// </summary>
    public Product? Product { get; private set; }

    /// <summary>
    /// Constructor untuk membuat item transaksi baru
    /// </summary>
    public TransactionItem(Guid productId, int quantity, decimal priceAtMoment)
    {
        ProductId = productId;
        Quantity = quantity;
        PriceAtMoment = priceAtMoment;
        Subtotal = quantity * priceAtMoment;
    }

    /// <summary>
    /// Constructor untuk EF Core
    /// </summary>
    private TransactionItem() { }

    /// <summary>
    /// Menghitung subtotal berdasarkan quantity dan price
    /// </summary>
    public void CalculateSubtotal()
    {
        Subtotal = Quantity * PriceAtMoment;
    }

    /// <summary>
    /// Mengecek apakah subtotal konsisten
    /// </summary>
    public bool IsSubtotalValid()
    {
        return Subtotal == (Quantity * PriceAtMoment);
    }
}
