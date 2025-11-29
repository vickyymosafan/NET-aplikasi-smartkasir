using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Application.DTOs;
using SmartKasir.Client.Services;
using SmartKasir.Core.Enums;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk UserManagementView - CRUD user (Admin only)
/// Requirements: 8.1, 8.2, 8.3, 8.4, 8.5
/// </summary>
public partial class UserManagementViewModel : BaseManagementViewModel<UserDto>
{
    private readonly ISmartKasirApi _api;

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
    private bool showResetPasswordDialog;

    public Array UserRoles => Enum.GetValues(typeof(UserRole));

    public UserManagementViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    public override async Task LoadDataAsync() => await LoadUsersAsync();

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await _api.GetUsersAsync();
            Items = new ObservableCollection<UserDto>(result);
        });
    }

    public override async Task SaveItemAsync() => await SaveUserAsync();

    [RelayCommand]
    public async Task SaveUserAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            if (IsEditing && SelectedItem != null)
            {
                var request = new UpdateUserRequest
                {
                    Username = FormUsername,
                    Role = FormRole,
                    IsActive = FormIsActive
                };
                await _api.UpdateUserAsync(SelectedItem.Id, request);
            }
            else
            {
                var request = new CreateUserRequest
                {
                    Username = FormUsername,
                    Password = FormPassword,
                    Role = FormRole
                };
                await _api.CreateUserAsync(request);
            }

            ShowForm = false;
            ClearFormFields();
            await LoadUsersAsync();
        });
    }

    /// <summary>
    /// Deactivate user - prevents login (Requirement 8.4)
    /// </summary>
    [RelayCommand]
    public async Task ToggleUserActiveAsync()
    {
        if (SelectedItem == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            var request = new UpdateUserRequest { IsActive = !SelectedItem.IsActive };
            await _api.UpdateUserAsync(SelectedItem.Id, request);
            await LoadUsersAsync();
        });
    }

    [RelayCommand]
    public void ShowResetPassword()
    {
        if (SelectedItem == null) return;
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
        if (SelectedItem == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            await _api.ResetPasswordAsync(SelectedItem.Id);
            ShowResetPasswordDialog = false;
        }, $"Password untuk {SelectedItem.Username} telah direset");
    }

    protected override void PopulateFormFromItem(UserDto item)
    {
        FormUsername = item.Username;
        FormPassword = string.Empty; // Don't show password
        FormRole = item.Role;
        FormIsActive = item.IsActive;
    }

    protected override void ClearFormFields()
    {
        FormUsername = string.Empty;
        FormPassword = string.Empty;
        FormRole = UserRole.Cashier;
        FormIsActive = true;
        SelectedItem = null;
        ClearMessages();
    }

    protected override string GetConflictErrorMessage() => "Username sudah digunakan";
}
