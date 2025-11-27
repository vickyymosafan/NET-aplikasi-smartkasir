namespace SmartKasir.Core.Entities;

/// <summary>
/// Entitas kategori produk
/// </summary>
public class Category
{
    /// <summary>
    /// Identifier unik kategori
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Nama kategori
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Koleksi produk dalam kategori ini
    /// </summary>
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    /// <summary>
    /// Constructor untuk membuat kategori baru
    /// </summary>
    public Category(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Constructor untuk EF Core
    /// </summary>
    private Category() { }

    /// <summary>
    /// Mengubah nama kategori
    /// </summary>
    public void UpdateName(string newName)
    {
        Name = newName;
    }

    /// <summary>
    /// Mengecek apakah kategori memiliki produk
    /// </summary>
    public bool HasProducts()
    {
        return Products.Any();
    }
}
