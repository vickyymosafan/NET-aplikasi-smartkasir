using System.Windows;
using System.Windows.Controls;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

/// <summary>
/// Code-behind untuk UserManagementView
/// </summary>
public partial class UserManagementView : Window
{
    private readonly UserManagementViewModel _viewModel;

    public UserManagementView(UserManagementViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadUsersAsync();
    }

    // Handle PasswordBox binding (PasswordBox doesn't support binding for security)
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            // Use reflection or a helper to set password in ViewModel
            // This is a common WPF pattern for PasswordBox
            var property = _viewModel.GetType().GetProperty("FormPassword");
            property?.SetValue(_viewModel, passwordBox.Password);
        }
    }
}
