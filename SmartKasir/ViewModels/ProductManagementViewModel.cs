using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;
using CategoryDto = SmartKasir.Client.Services.CategoryDto;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk ProductManagementView - CRUD produk (Admin only)
/// Requirements: 6.1, 6.2, 6.3, 6.4, 6.5
/// </summary>
public partial class ProductManagementViewModel : ObservableObject
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private int pageSize = 50;

    // Form fields for create/edit
    [ObservableProperty]
    private string formBarcode = string.Empty;

    [ObservableProperty]
    private string formName = string.Empty;

    [ObservableProperty]
    private decimal formPrice;

    [ObservableProperty]
    private int formStockQty;

    [ObservableProperty]
    private int formCategoryId;

    [ObservableProperty]
    private bool formIsActive = true;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool showForm;

    public ProductManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }


    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await _api.GetProductsAsync(CurrentPage, PageSize);
            Products = new ObservableCollection<ProductDto>(result.Items);
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / PageSize);
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
    public async Task LoadCategoriesAsync()
    {
        try
        {
            var cats = await _api.GetCategoriesAsync();
            Categories = new ObservableCollection<CategoryDto>(cats);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat kategori: {ex.Message}";
        }
    }

    [RelayCommand]
    public void ShowCreateForm()
    {
        ClearForm();
        IsEditing = false;
        ShowForm = true;
    }

    [RelayCommand]
    public void ShowEditForm()
    {
        if (SelectedProduct == null) return;

        FormBarcode = SelectedProduct.Barcode;
        FormName = SelectedProduct.Name;
        FormPrice = SelectedProduct.Price;
        FormStockQty = SelectedProduct.StockQty;
        FormCategoryId = SelectedProduct.CategoryId;
        FormIsActive = SelectedProduct.IsActive;
        IsEditing = true;
        ShowForm = true;
    }

    [RelayCommand]
    public void CancelForm()
    {
        ClearForm();
        ShowForm = false;
    }

    [RelayCommand]
    public async Task SaveProductAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            if (IsEditing && SelectedProduct != null)
            {
                // Update existing product
                var request = new UpdateProductRequest(
                    FormName,
                    FormPrice,
                    FormStockQty,
                    FormCategoryId,
                    FormIsActive);

                await _api.UpdateProductAsync(SelectedProduct.Id, request);
            }
            else
            {
                // Create new product - barcode validation handled by server (6.3)
                var request = new CreateProductRequest(
                    FormBarcode,
                    FormName,
                    FormPrice,
                    FormStockQty,
                    FormCategoryId);

                await _api.CreateProductAsync(request);
            }

            ShowForm = false;
            ClearForm();
            await LoadProductsAsync();
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "Barcode sudah digunakan oleh produk lain";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menyimpan produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Soft delete - set IsActive = false (Requirement 6.5)
    /// </summary>
    [RelayCommand]
    public async Task DeactivateProductAsync()
    {
        if (SelectedProduct == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var request = new UpdateProductRequest(null, null, null, null, false);
            await _api.UpdateProductAsync(SelectedProduct.Id, request);
            await LoadProductsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menonaktifkan produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadProductsAsync();
        }
    }

    [RelayCommand]
    public async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProductsAsync();
        }
    }

    private void ClearForm()
    {
        FormBarcode = string.Empty;
        FormName = string.Empty;
        FormPrice = 0;
        FormStockQty = 0;
        FormCategoryId = 0;
        FormIsActive = true;
        SelectedProduct = null;
    }
}
