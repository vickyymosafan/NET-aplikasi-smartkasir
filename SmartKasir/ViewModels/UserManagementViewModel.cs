using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using SmartKasir.Core.Enums;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk UserManagementView - CRUD user (Admin only)
/// Requirements: 8.1, 8.2, 8.3, 8.4, 8.5
/// </summary>
public partial class UserManagementViewModel : ObservableObject
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private ObservableCollection<UserDto> users = new();

    [ObservableProperty]
    private UserDto? selectedUser;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    // Form fields
    [ObservableProperty]
    private string formUsername = string.Empty;

    [ObservableProperty]
    private string formPassword = string.Empty;

    [ObservableProperty]
    private UserRole formRole = UserRole.Cashier;

    [ObservableProperty]
    private bool formIsActive = true;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool showForm;

    [ObservableProperty]
    private bool showResetPasswordDialog;

    public Array UserRoles => Enum.GetValues(typeof(UserRole));

    public UserManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await _api.GetUsersAsync();
            Users = new ObservableCollection<UserDto>(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
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
        if (SelectedUser == null) return;

        FormUsername = SelectedUser.Username;
        FormPassword = string.Empty; // Don't show password
        FormRole = SelectedUser.Role;
        FormIsActive = SelectedUser.IsActive;
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
    public async Task SaveUserAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            if (IsEditing && SelectedUser != null)
            {
                // Update existing user (Requirement 8.3)
                var request = new UpdateUserRequest(
                    FormUsername,
                    FormRole,
                    FormIsActive);

                await _api.UpdateUserAsync(SelectedUser.Id, request);
            }
            else
            {
                // Create new user with hashed password (Requirement 8.2)
                var request = new CreateUserRequest(
                    FormUsername,
                    FormPassword,
                    FormRole);

                await _api.CreateUserAsync(request);
            }

            ShowForm = false;
            ClearForm();
            await LoadUsersAsync();
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            ErrorMessage = "Username sudah digunakan";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal menyimpan user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deactivate user - prevents login (Requirement 8.4)
    /// </summary>
    [RelayCommand]
    public async Task ToggleUserActiveAsync()
    {
        if (SelectedUser == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var request = new UpdateUserRequest(null, null, !SelectedUser.IsActive);
            await _api.UpdateUserAsync(SelectedUser.Id, request);
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal mengubah status user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ShowResetPassword()
    {
        if (SelectedUser == null) return;
        ShowResetPasswordDialog = true;
    }

    [RelayCommand]
    public void CancelResetPassword()
    {
        ShowResetPasswordDialog = false;
    }

    /// <summary>
    /// Reset password - generates new hashed password (Requirement 8.5)
    /// </summary>
    [RelayCommand]
    public async Task ConfirmResetPasswordAsync()
    {
        if (SelectedUser == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await _api.ResetPasswordAsync(SelectedUser.Id);
            
            ShowResetPasswordDialog = false;
            SuccessMessage = $"Password untuk {SelectedUser.Username} telah direset";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal reset password: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearForm()
    {
        FormUsername = string.Empty;
        FormPassword = string.Empty;
        FormRole = UserRole.Cashier;
        FormIsActive = true;
        SelectedUser = null;
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
