using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.Services;

public class CartService : ICartService
{
    private readonly List<CartItem> _items = new();
    private const decimal TaxRate = 0.10m;

    public List<CartItem> Items => _items;
    public decimal Subtotal => _items.Sum(i => i.Subtotal);
    public decimal Tax => Subtotal * TaxRate;
    public decimal Total => Subtotal + Tax;
    public event Action? OnCartChanged;

    public void AddItem(ProductDto product)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            _items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Barcode = product.Barcode,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }
        OnCartChanged?.Invoke();
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            OnCartChanged?.Invoke();
        }
    }

    public void IncreaseQuantity(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity++;
            OnCartChanged?.Invoke();
        }
    }

    public void DecreaseQuantity(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            item.Quantity--;
            if (item.Quantity <= 0)
            {
                _items.Remove(item);
            }
            OnCartChanged?.Invoke();
        }
    }

    public void Clear()
    {
        _items.Clear();
        OnCartChanged?.Invoke();
    }
}
