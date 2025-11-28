using System.Windows.Controls;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class ProductGridView : UserControl
{
    public ProductGridView(ProductGridViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
