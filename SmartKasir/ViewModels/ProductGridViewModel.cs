using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using SmartKasir.Application.DTOs;
using System.Collections.ObjectModel;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk ProductGridView
/// </summary>
public partial class ProductGridViewModel : ObservableObject
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public event EventHandler<ProductDto>? ProductSelected;

    public ProductGridViewModel(IProductService productService)
    {
        _productService = productService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var products = await _productService.GetAllAsync();
            Products = new ObservableCollection<ProductDto>(products);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            await LoadProductsAsync();
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var product = await _productService.GetByBarcodeAsync(barcode);
            if (product != null)
            {
                Products = new ObservableCollection<ProductDto> { product };
                ProductSelected?.Invoke(this, product);
            }
            else
            {
                ErrorMessage = "Produk tidak ditemukan";
                Products.Clear();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal mencari produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchByKeywordAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
        {
            await LoadProductsAsync();
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var products = await _productService.SearchAsync(SearchKeyword);
            Products = new ObservableCollection<ProductDto>(products);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal mencari produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void SelectProduct(ProductDto product)
    {
        ProductSelected?.Invoke(this, product);
    }
}
