using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk ProductManagementView - CRUD produk (Admin only)
/// Requirements: 6.1, 6.2, 6.3, 6.4, 6.5
/// </summary>
public partial class ProductManagementViewModel : BaseManagementViewModel<ProductDto>
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

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

    public ProductManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    public override async Task LoadDataAsync() => await LoadProductsAsync();

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _api.GetProductsAsync(CurrentPage, PageSize);
            Items = new ObservableCollection<ProductDto>(result.Items);
            TotalPages = (int)Math.Ceiling((double)result.TotalCount / PageSize);
        });
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

    public override async Task SaveItemAsync() => await SaveProductAsync();

    [RelayCommand]
    public async Task SaveProductAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            if (IsEditing && SelectedItem != null)
            {
                var request = new UpdateProductRequest(
                    FormName, FormPrice, FormStockQty, FormCategoryId, FormIsActive);
                await _api.UpdateProductAsync(SelectedItem.Id, request);
            }
            else
            {
                var request = new CreateProductRequest(
                    FormBarcode, FormName, FormPrice, FormStockQty, FormCategoryId);
                await _api.CreateProductAsync(request);
            }

            ShowForm = false;
            ClearFormFields();
            await LoadProductsAsync();
        });
    }

    /// <summary>
    /// Soft delete - set IsActive = false (Requirement 6.5)
    /// </summary>
    [RelayCommand]
    public async Task DeactivateProductAsync()
    {
        if (SelectedItem == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var request = new UpdateProductRequest(null, null, null, null, false);
            await _api.UpdateProductAsync(SelectedItem.Id, request);
            await LoadProductsAsync();
        });
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

    protected override void PopulateFormFromItem(ProductDto item)
    {
        FormBarcode = item.Barcode;
        FormName = item.Name;
        FormPrice = item.Price;
        FormStockQty = item.StockQty;
        FormCategoryId = item.CategoryId;
        FormIsActive = item.IsActive;
    }

    protected override void ClearFormFields()
    {
        FormBarcode = string.Empty;
        FormName = string.Empty;
        FormPrice = 0;
        FormStockQty = 0;
        FormCategoryId = 0;
        FormIsActive = true;
        SelectedItem = null;
    }

    protected override string GetConflictErrorMessage() => "Barcode sudah digunakan oleh produk lain";
}
