using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }
}
