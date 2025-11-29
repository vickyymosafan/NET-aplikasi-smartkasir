using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;
using SmartKasir.Core.Enums;

namespace SmartKasir.Client.Views;

public partial class DashboardView : UserControl
{
    private readonly IAuthService? _authService;
    private readonly IProductService? _productService;
    private ObservableCollection<ProductDto> _products = new();

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
        
        // Setup UI based on user role
        SetupRoleBasedUI();
        
        // Load products
        await LoadProductsAsync();
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
            EmptyStateHint.Text = "Hubungi Admin untuk menambahkan produk";
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
            
            // Show/hide empty state
            EmptyState.Visibility = _products.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            
            Console.WriteLine($"[DashboardView] Loaded {_products.Count} products");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DashboardView] Error loading products: {ex.Message}");
        }
    }

    private void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        // Only Admin can add products
        if (_authService?.CurrentUser?.IsAdmin != true)
        {
            MessageBox.Show("Hanya Admin yang dapat menambahkan produk", "Akses Ditolak", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Clear form
        NewBarcode.Text = "";
        NewName.Text = "";
        NewPrice.Text = "";
        NewStock.Text = "";
        NewCategory.Text = "Umum";
        ErrorText.Visibility = Visibility.Collapsed;
        
        // Show dialog
        AddProductDialog.Visibility = Visibility.Visible;
    }

    private void CancelAddProduct_Click(object sender, RoutedEventArgs e)
    {
        AddProductDialog.Visibility = Visibility.Collapsed;
    }

    private async void SaveProduct_Click(object sender, RoutedEventArgs e)
    {
        if (_productService == null) return;

        // Validate
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
            Console.WriteLine($"[DashboardView] Creating product: {name}");
            await _productService.CreateLocalAsync(barcode, name, price, stock, category);
            
            // Close dialog and refresh
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

    private void ProductCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is ProductDto product)
        {
            Console.WriteLine($"[DashboardView] Product clicked: {product.Name}");
            // TODO: Add to cart
            MessageBox.Show($"Produk: {product.Name}\nHarga: Rp {product.Price:N0}\nStok: {product.StockQty}", "Detail Produk", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

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
}
