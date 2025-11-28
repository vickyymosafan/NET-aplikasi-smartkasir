using System.Windows.Controls;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class CartView : UserControl
{
    public CartView(CartViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
