using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// Base ViewModel untuk CRUD management views
/// Mengurangi duplikasi code di ProductManagementViewModel, CategoryManagementViewModel, UserManagementViewModel
/// </summary>
public abstract partial class BaseManagementViewModel<TDto> : ObservableObject where TDto : class
{
    [ObservableProperty]
    private ObservableCollection<TDto> items = new();

    [ObservableProperty]
    private TDto? selectedItem;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool showForm;

    /// <summary>
    /// Load data dari server/source
    /// </summary>
    public abstract Task LoadDataAsync();

    /// <summary>
    /// Save item (create atau update)
    /// </summary>
    public abstract Task SaveItemAsync();

    /// <summary>
    /// Populate form fields dari selected item untuk editing
    /// </summary>
    protected abstract void PopulateFormFromItem(TDto item);

    /// <summary>
    /// Clear semua form fields
    /// </summary>
    protected abstract void ClearFormFields();

    [RelayCommand]
    public virtual void ShowCreateForm()
    {
        ClearFormFields();
        IsEditing = false;
        ShowForm = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    [RelayCommand]
    public virtual void ShowEditForm()
    {
        if (SelectedItem == null) return;

        PopulateFormFromItem(SelectedItem);
        IsEditing = true;
        ShowForm = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    [RelayCommand]
    public virtual void CancelForm()
    {
        ClearFormFields();
        ShowForm = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Helper untuk menjalankan operasi async dengan loading state dan error handling
    /// </summary>
    protected async Task ExecuteWithLoadingAsync(Func<Task> operation, string? successMsg = null)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await operation();

            if (!string.IsNullOrEmpty(successMsg))
            {
                SuccessMessage = successMsg;
            }
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = GetConflictErrorMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Terjadi kesalahan: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Override untuk custom conflict error message
    /// </summary>
    protected virtual string GetConflictErrorMessage() => "Data sudah ada";

    /// <summary>
    /// Clear messages
    /// </summary>
    protected void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
