using System.Windows;
using System.Windows.Controls;
using SmartKasir.Client.Services;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class DashboardView : UserControl
{
    private readonly IAuthService? _authService;

    public DashboardView()
    {
        InitializeComponent();
    }

    public DashboardView(DashboardViewModel viewModel, IAuthService authService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _authService = authService;
    }

    private async void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_authService != null)
        {
            await _authService.LogoutAsync();
        }
    }
}
