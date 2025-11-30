using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

public interface ICartService
{
    List<CartItem> Items { get; }
    decimal Subtotal { get; }
    decimal Tax { get; }
    decimal Total { get; }
    event Action? OnCartChanged;

    void AddItem(ProductDto product);
    void RemoveItem(Guid productId);
    void IncreaseQuantity(Guid productId);
    void DecreaseQuantity(Guid productId);
    void Clear();
}

public class CartItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string Barcode { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => UnitPrice * Quantity;
}
