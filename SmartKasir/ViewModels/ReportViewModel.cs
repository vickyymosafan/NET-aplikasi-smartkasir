using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using Microsoft.Win32;
using System.IO;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk ReportView - Laporan penjualan (Admin only)
/// Requirements: 9.1, 9.2, 9.3, 9.4, 9.5
/// </summary>
public partial class ReportViewModel : ObservableObject
{
    private readonly ISmartKasirApi _api;

    [ObservableProperty]
    private DateTime startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime endDate = DateTime.Today;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? successMessage;

    // Daily Sales Report
    [ObservableProperty]
    private DailySalesReportDto? dailySalesReport;

    // Product Sales Report
    [ObservableProperty]
    private ProductSalesReportDto? productSalesReport;

    [ObservableProperty]
    private ObservableCollection<ProductSalesDetail> productSalesItems = new();

    [ObservableProperty]
    private string selectedReportType = "Daily";

    [ObservableProperty]
    private bool isExporting;

    public string[] ReportTypes => new[] { "Daily", "Product" };

    public ReportViewModel(ISmartKasirApi api)
    {
        _api = api;
    }

    /// <summary>
    /// Load daily sales report (Requirement 9.2)
    /// </summary>
    [RelayCommand]
    public async Task LoadDailySalesReportAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            DailySalesReport = await _api.GetDailySalesReportAsync(SelectedDate);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat laporan harian: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load product sales report (Requirement 9.3)
    /// </summary>
    [RelayCommand]
    public async Task LoadProductSalesReportAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            ProductSalesReport = await _api.GetProductSalesReportAsync(StartDate, EndDate);
            
            if (ProductSalesReport != null)
            {
                ProductSalesItems = new ObservableCollection<ProductSalesDetail>(ProductSalesReport.Products);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal memuat laporan produk: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadReportAsync()
    {
        if (SelectedReportType == "Daily")
        {
            await LoadDailySalesReportAsync();
        }
        else
        {
            await LoadProductSalesReportAsync();
        }
    }


    /// <summary>
    /// Export to PDF - async, non-blocking (Requirement 9.4)
    /// </summary>
    [RelayCommand]
    public async Task ExportToPdfAsync()
    {
        try
        {
            IsExporting = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var saveDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = $"Laporan_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var content = await _api.ExportReportToPdfAsync(StartDate, EndDate);
                var bytes = await content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                
                SuccessMessage = $"Laporan berhasil diekspor ke {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal ekspor PDF: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    /// <summary>
    /// Export to Excel - async, non-blocking (Requirement 9.5)
    /// </summary>
    [RelayCommand]
    public async Task ExportToExcelAsync()
    {
        try
        {
            IsExporting = true;
            ErrorMessage = null;
            SuccessMessage = null;

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Laporan_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var content = await _api.ExportReportToExcelAsync(StartDate, EndDate);
                var bytes = await content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
                
                SuccessMessage = $"Laporan berhasil diekspor ke {saveDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Gagal ekspor Excel: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }
}
