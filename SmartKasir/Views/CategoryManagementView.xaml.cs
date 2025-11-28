using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

/// <summary>
/// Code-behind untuk CategoryManagementView
/// </summary>
public partial class CategoryManagementView : Window
{
    public CategoryManagementView(CategoryManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CategoryManagementViewModel vm)
        {
            await vm.LoadCategoriesAsync();
        }
    }
}
