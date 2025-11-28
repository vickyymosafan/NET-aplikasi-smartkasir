using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Application.DTOs;
using System.Collections.ObjectModel;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk CartView
/// </summary>
public partial class CartViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> items = new();

    [ObservableProperty]
    private decimal subtotal = 0;

    [ObservableProperty]
    private decimal tax = 0;

    [ObservableProperty]
    private decimal total = 0;

    public event EventHandler<CartItemViewModel>? ItemRemoved;
    public event EventHandler? CartCleared;

    public CartViewModel()
    {
    }

    [RelayCommand]
    public void AddItem(ProductDto product)
    {
        AddItemWithQuantity(product, 1);
    }

    public void AddItemWithQuantity(ProductDto product, int quantity)
    {
        if (product == null || quantity <= 0)
            return;

        // Check if product already in cart
        var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            Items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = quantity
            });
        }

        RecalculateTotal();
    }

    [RelayCommand]
    public void RemoveItem(CartItemViewModel item)
    {
        if (Items.Remove(item))
        {
            ItemRemoved?.Invoke(this, item);
            RecalculateTotal();
        }
    }

    [RelayCommand]
    public void UpdateQuantity(CartItemViewModel item)
    {
        // Recalculate when quantity changes
        RecalculateTotal();
    }

    public void SetItemQuantity(CartItemViewModel item, int newQuantity)
    {
        if (newQuantity <= 0)
        {
            RemoveItem(item);
        }
        else
        {
            item.Quantity = newQuantity;
            RecalculateTotal();
        }
    }

    [RelayCommand]
    public void Clear()
    {
        Items.Clear();
        RecalculateTotal();
        CartCleared?.Invoke(this, EventArgs.Empty);
    }

    private void RecalculateTotal()
    {
        Subtotal = Items.Sum(i => i.Subtotal);
        Tax = Subtotal * 0.1m; // 10% tax
        Total = Subtotal + Tax;
    }
}

/// <summary>
/// ViewModel untuk item dalam cart
/// </summary>
public partial class CartItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal subtotal;

    partial void OnQuantityChanged(int value)
    {
        Subtotal = UnitPrice * value;
    }

    partial void OnUnitPriceChanged(decimal value)
    {
        Subtotal = value * Quantity;
    }
}
