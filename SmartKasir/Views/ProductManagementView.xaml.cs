using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

/// <summary>
/// Code-behind untuk ProductManagementView
/// </summary>
public partial class ProductManagementView : Window
{
    public ProductManagementView(ProductManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductManagementViewModel vm)
        {
            await vm.LoadCategoriesAsync();
            await vm.LoadProductsAsync();
        }
    }
}
