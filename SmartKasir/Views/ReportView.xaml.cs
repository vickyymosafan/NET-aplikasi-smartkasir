using System.Windows;
using SmartKasir.Client.ViewModels;

namespace SmartKasir.Client.Views;

/// <summary>
/// Code-behind untuk ReportView
/// </summary>
public partial class ReportView : Window
{
    public ReportView(ReportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
