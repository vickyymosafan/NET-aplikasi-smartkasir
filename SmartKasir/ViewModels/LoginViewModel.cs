using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk LoginView
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username dan password harus diisi";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);

            if (result.Success)
            {
                _navigationService.NavigateTo("Dashboard");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login gagal";
            }
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
}
