using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class CheckoutDialog : Window
{
    public CheckoutDialog(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
