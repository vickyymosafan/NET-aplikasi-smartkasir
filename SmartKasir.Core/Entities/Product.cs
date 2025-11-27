namespace SmartKasir.Core.Entities;

/// <summary>
/// Entitas produk dalam katalog
/// </summary>
public class Product
{
    /// <summary>
    /// Identifier unik produk
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Barcode produk (harus unik)
    /// </summary>
    public string Barcode { get; private set; } = string.Empty;

    /// <summary>
    /// Nama produk
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Harga jual produk
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// Jumlah stok produk
    /// </summary>
    public int StockQty { get; private set; }

    /// <summary>
    /// Foreign key ke kategori
    /// </summary>
    public int CategoryId { get; private set; }

    /// <summary>
    /// Status aktif produk
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Navigation property ke kategori
    /// </summary>
    public Category? Category { get; private set; }

    /// <summary>
    /// Constructor untuk membuat produk baru
    /// </summary>
    public Product(string barcode, string name, decimal price, int stockQty, int categoryId)
    {
        Id = Guid.NewGuid();
        Barcode = barcode;
        Name = name;
        Price = price;
        StockQty = stockQty;
        CategoryId = categoryId;
        IsActive = true;
    }

    /// <summary>
    /// Constructor untuk EF Core
    /// </summary>
    private Product() { }

    /// <summary>
    /// Mengubah harga produk
    /// </summary>
    public void UpdatePrice(decimal newPrice)
    {
        Price = newPrice;
    }

    /// <summary>
    /// Mengubah nama produk
    /// </summary>
    public void UpdateName(string newName)
    {
        Name = newName;
    }

    /// <summary>
    /// Menambah stok produk
    /// </summary>
    public void AddStock(int quantity)
    {
        StockQty += quantity;
    }

    /// <summary>
    /// Mengurangi stok produk
    /// </summary>
    public void DecrementStock(int quantity)
    {
        if (StockQty < quantity)
            throw new InvalidOperationException("Stok tidak cukup");
        
        StockQty -= quantity;
    }

    /// <summary>
    /// Menonaktifkan produk (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Mengaktifkan produk
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Mengecek apakah stok cukup
    /// </summary>
    public bool HasSufficientStock(int quantity)
    {
        return StockQty >= quantity;
    }
}
