using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk CategoryManagementView - CRUD kategori (Admin only)
/// Requirements: 7.1, 7.2, 7.3, 7.4
/// </summary>
public partial class CategoryManagementViewModel : BaseManagementViewModel<CategoryDto>
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private string formName = string.Empty;

    [ObservableProperty]
    private bool showDeleteWarning;

    [ObservableProperty]
    private string deleteWarningMessage = string.Empty;

    public CategoryManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    public override async Task LoadDataAsync() => await LoadCategoriesAsync();

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _api.GetCategoriesAsync();
            Items = new ObservableCollection<CategoryDto>(result);
        });
    }

    public override async Task SaveItemAsync() => await SaveCategoryAsync();

    [RelayCommand]
    public async Task SaveCategoryAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            if (IsEditing && SelectedItem != null)
            {
                var request = new UpdateCategoryRequest { Name = FormName };
                await _api.UpdateCategoryAsync(SelectedItem.Id, request);
            }
            else
            {
                var request = new CreateCategoryRequest { Name = FormName };
                await _api.CreateCategoryAsync(request);
            }

            ShowForm = false;
            ClearFormFields();
            await LoadCategoriesAsync();
        });
    }

    /// <summary>
    /// Check referential integrity before delete (Requirement 7.4)
    /// </summary>
    [RelayCommand]
    public void RequestDeleteCategory()
    {
        if (SelectedItem == null) return;

        ShowDeleteWarning = true;
        DeleteWarningMessage = $"Apakah Anda yakin ingin menghapus kategori '{SelectedItem.Name}'?";
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
        if (SelectedItem == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            ShowDeleteWarning = false;
            await _api.DeleteCategoryAsync(SelectedItem.Id);
            await LoadCategoriesAsync();
        });
    }

    protected override void PopulateFormFromItem(CategoryDto item)
    {
        FormName = item.Name;
    }

    protected override void ClearFormFields()
    {
        FormName = string.Empty;
        SelectedItem = null;
    }

    protected override string GetConflictErrorMessage() => 
        "Tidak dapat menghapus kategori yang masih memiliki produk";
}
