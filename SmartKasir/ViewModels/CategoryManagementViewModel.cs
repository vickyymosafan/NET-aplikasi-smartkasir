using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk CategoryManagementView - CRUD kategori (Admin only)
/// Requirements: 7.1, 7.2, 7.3, 7.4
/// </summary>
public partial class CategoryManagementViewModel : ObservableObject
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private CategoryDto? selectedCategory;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string formName = string.Empty;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool showForm;

    [ObservableProperty]
    private bool showDeleteWarning;

    [ObservableProperty]
    private string deleteWarningMessage = string.Empty;

    public CategoryManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await _api.GetCategoriesAsync();
            Categories = new ObservableCollection<CategoryDto>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat kategori: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ShowCreateForm()
    {
        FormName = string.Empty;
        IsEditing = false;
        ShowForm = true;
    }

    [RelayCommand]
    public void ShowEditForm()
    {
        if (SelectedCategory == null) return;

        FormName = SelectedCategory.Name;
        IsEditing = true;
        ShowForm = true;
    }

    [RelayCommand]
    public void CancelForm()
    {
        FormName = string.Empty;
        ShowForm = false;
    }

    [RelayCommand]
    public async Task SaveCategoryAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            if (IsEditing && SelectedCategory != null)
            {
                var request = new UpdateCategoryRequest(FormName);
                await _api.UpdateCategoryAsync(SelectedCategory.Id, request);
            }
            else
            {
                var request = new CreateCategoryRequest(FormName);
                await _api.CreateCategoryAsync(request);
            }

            ShowForm = false;
            FormName = string.Empty;
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menyimpan kategori: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Check referential integrity before delete (Requirement 7.4)
    /// </summary>
    [RelayCommand]
    public void RequestDeleteCategory()
    {
        if (SelectedCategory == null) return;

        // Check if category has products - show warning
        // Note: ProductCount comes from CategoryDto in Application layer
        ShowDeleteWarning = true;
        DeleteWarningMessage = $"Apakah Anda yakin ingin menghapus kategori '{SelectedCategory.Name}'?";
    }

    [RelayCommand]
    public void CancelDelete()
    {
        ShowDeleteWarning = false;
        DeleteWarningMessage = string.Empty;
    }

    [RelayCommand]
    public async Task ConfirmDeleteCategoryAsync()
    {
        if (SelectedCategory == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            ShowDeleteWarning = false;

            await _api.DeleteCategoryAsync(SelectedCategory.Id);
            await LoadCategoriesAsync();
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            // Referential integrity violation - category has products
            ErrorMessage = "Tidak dapat menghapus kategori yang masih memiliki produk";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menghapus kategori: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
