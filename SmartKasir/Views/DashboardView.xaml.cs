using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;

namespace SmartKasir.Client.Views;

/// <summary>
/// Simple cart item for display
/// </summary>
public class CartItem
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
}

public partial class DashboardView : UserControl
{
    private readonly IAuthService? _authService;
    private readonly IProductService? _productService;
    private ObservableCollection<ProductDto> _products = new();
    private ObservableCollection<CartItem> _cartItems = new();

    public DashboardView()
    {
        InitializeComponent();
    }

    public DashboardView(IAuthService authService, IProductService productService)
    {
        InitializeComponent();
        _authService = authService;
        _productService = productService;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("[DashboardView] OnLoaded called");
        SetupRoleBasedUI();
        await LoadProductsAsync();
        UpdateCartDisplay();
    }

    private void SetupRoleBasedUI()
    {
        if (_authService?.CurrentUser == null) return;

        var user = _authService.CurrentUser;
        UsernameText.Text = user.Username;

        Console.WriteLine($"[DashboardView] User: {user.Username}, Role: {user.Role}");

        if (user.IsAdmin)
        {
            RoleText.Text = "Dashboard Admin";
            AddProductButton.Visibility = Visibility.Visible;
            EmptyStateHint.Text = "Klik 'Tambah Produk' untuk menambahkan";
        }
        else
        {
            RoleText.Text = "Dashboard Kasir";
            AddProductButton.Visibility = Visibility.Collapsed;
            EmptyStateHint.Text = "Klik produk untuk menambahkan ke keranjang";
        }
    }

    private async Task LoadProductsAsync()
    {
        if (_productService == null) return;

        try
        {
            Console.WriteLine("[DashboardView] Loading products...");
            var products = await _productService.GetAllAsync();
            _products = new ObservableCollection<ProductDto>(products);

            ProductList.ItemsSource = _products;
            ProductCountText.Text = $" ({_products.Count})";
            EmptyState.Visibility = _products.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            Console.WriteLine($"[DashboardView] Loaded {_products.Count} products");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardView] Error loading products: {ex.Message}");
        }
    }

    private void ProductCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is ProductDto product)
        {
            Console.WriteLine($"[DashboardView] Adding to cart: {product.Name}");
            AddToCart(product);
        }
    }

    private void AddToCart(ProductDto product)
    {
        // Check stock
        if (product.StockQty <= 0)
        {
            MessageBox.Show("Stok produk habis!", "Stok Kosong", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Check if already in cart
        var existingItem = _cartItems.FirstOrDefault(c => c.ProductId == product.Id);
        if (existingItem != null)
        {
            // Check if we can add more
            if (existingItem.Quantity >= product.StockQty)
            {
                MessageBox.Show($"Stok tidak mencukupi! Tersedia: {product.StockQty}", "Stok Terbatas", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            existingItem.Quantity++;
        }
        else
        {
            _cartItems.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }

        UpdateCartDisplay();
        Console.WriteLine($"[DashboardView] Cart updated: {_cartItems.Count} items");
    }

    private void UpdateCartDisplay()
    {
        // Update item count
        var totalItems = _cartItems.Sum(c => c.Quantity);
        CartItemCount.Text = $"{totalItems} item";

        // Calculate totals
        var subtotal = _cartItems.Sum(c => c.Subtotal);
        var tax = subtotal * 0.1m;
        var total = subtotal + tax;

        SubtotalText.Text = $"Rp {subtotal:N0}";
        TaxText.Text = $"Rp {tax:N0}";
        TotalText.Text = $"Rp {total:N0}";

        // Update cart items display
        CartItemsPanel.Children.Clear();

        if (_cartItems.Count == 0)
        {
            EmptyCartMessage.Visibility = Visibility.Visible;
        }
        else
        {
            EmptyCartMessage.Visibility = Visibility.Collapsed;

            foreach (var item in _cartItems)
            {
                var itemBorder = CreateCartItemUI(item);
                CartItemsPanel.Children.Add(itemBorder);
            }
        }
    }

    private Border CreateCartItemUI(CartItem item)
    {
        var border = new Border
        {
            Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2a2a2a")),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 8)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var leftStack = new StackPanel();
        leftStack.Children.Add(new TextBlock
        {
            Text = item.Name,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            FontWeight = FontWeights.Medium,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        leftStack.Children.Add(new TextBlock
        {
            Text = $"Rp {item.Price:N0} x {item.Quantity}",
            Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#a0a0a0")),
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 0)
        });

        var rightStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        rightStack.Children.Add(new TextBlock
        {
            Text = $"Rp {item.Subtotal:N0}",
            Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6")),
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Right
        });

        // Remove button
        var removeBtn = new Button
        {
            Content = "✕",
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#ef4444")),
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            FontSize = 12,
            Padding = new Thickness(4),
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = item.ProductId
        };
        removeBtn.Click += RemoveCartItem_Click;
        rightStack.Children.Add(removeBtn);

        Grid.SetColumn(leftStack, 0);
        Grid.SetColumn(rightStack, 1);
        grid.Children.Add(leftStack);
        grid.Children.Add(rightStack);

        border.Child = grid;
        return border;
    }

    private void RemoveCartItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid productId)
        {
            var item = _cartItems.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _cartItems.Remove(item);
                }
                UpdateCartDisplay();
            }
        }
    }

    private void ClearCart_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0) return;

        var result = MessageBox.Show("Hapus semua item dari keranjang?", "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _cartItems.Clear();
            UpdateCartDisplay();
        }
    }

    private void Checkout_Click(object sender, RoutedEventArgs e)
    {
        if (_cartItems.Count == 0)
        {
            MessageBox.Show("Keranjang kosong!", "Checkout", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var subtotal = _cartItems.Sum(c => c.Subtotal);
        var tax = subtotal * 0.1m;
        var total = subtotal + tax;

        var result = MessageBox.Show(
            $"Total Pembayaran: Rp {total:N0}\n\nLanjutkan checkout?",
            "Konfirmasi Checkout",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // TODO: Process actual transaction
            MessageBox.Show($"Transaksi berhasil!\nTotal: Rp {total:N0}", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
            _cartItems.Clear();
            UpdateCartDisplay();
        }
    }

    #region Admin Product Management

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authService?.CurrentUser?.IsAdmin != true)
        {
            MessageBox.Show("Hanya Admin yang dapat menambahkan produk", "Akses Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        NewBarcode.Text = "";
        NewName.Text = "";
        NewPrice.Text = "";
        NewStock.Text = "";
        NewCategory.Text = "Umum";
        ErrorText.Visibility = Visibility.Collapsed;
        AddProductDialog.Visibility = Visibility.Visible;
    }

    private void CancelAddProduct_Click(object sender, RoutedEventArgs e)
    {
        AddProductDialog.Visibility = Visibility.Collapsed;
    }

    private async void SaveProduct_Click(object sender, RoutedEventArgs e)
    {
        if (_productService == null) return;

        var barcode = NewBarcode.Text.Trim();
        var name = NewName.Text.Trim();
        var priceText = NewPrice.Text.Trim();
        var stockText = NewStock.Text.Trim();
        var category = NewCategory.Text.Trim();

        if (string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(name))
        {
            ShowError("Barcode dan Nama Produk harus diisi");
            return;
        }

        if (!decimal.TryParse(priceText, out var price) || price < 0)
        {
            ShowError("Harga harus berupa angka positif");
            return;
        }

        if (!int.TryParse(stockText, out var stock) || stock < 0)
        {
            ShowError("Stok harus berupa angka positif");
            return;
        }

        try
        {
            await _productService.CreateLocalAsync(barcode, name, price, stock, category);
            AddProductDialog.Visibility = Visibility.Collapsed;
            await LoadProductsAsync();
            MessageBox.Show($"Produk '{name}' berhasil ditambahkan!", "Sukses", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    #endregion

    #region Search and Navigation

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        if (_productService == null) return;

        var keyword = SearchBox.Text.Trim();
        if (string.IsNullOrEmpty(keyword))
        {
            await LoadProductsAsync();
            return;
        }

        try
        {
            var results = await _productService.SearchAsync(keyword);
            _products = new ObservableCollection<ProductDto>(results);
            ProductList.ItemsSource = _products;
            ProductCountText.Text = $" ({_products.Count})";
            EmptyState.Visibility = _products.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardView] Search error: {ex.Message}");
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadProductsAsync();
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authService != null)
        {
            await _authService.LogoutAsync();
        }
    }

    #endregion
}
