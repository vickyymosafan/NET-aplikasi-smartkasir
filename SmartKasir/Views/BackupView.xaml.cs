using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

/// <summary>
/// Code-behind untuk BackupView
/// </summary>
public partial class BackupView : Window
{
    public BackupView(BackupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is BackupViewModel vm)
        {
            await vm.LoadBackupsAsync();
        }
    }
}
