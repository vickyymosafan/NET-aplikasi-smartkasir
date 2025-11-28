using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

public partial class DashboardView : Window
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
