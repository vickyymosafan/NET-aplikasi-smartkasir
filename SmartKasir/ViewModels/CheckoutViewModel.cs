using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Enums;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk CheckoutDialog
/// </summary>
public partial class CheckoutViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly IPrinterService _printerService;

    [ObservableProperty]
    private decimal totalAmount = 0;

    [ObservableProperty]
    private decimal amountPaid = 0;

    [ObservableProperty]
    private decimal change = 0;

    [ObservableProperty]
    private PaymentMethod selectedPaymentMethod = PaymentMethod.Cash;

    [ObservableProperty]
    private bool isProcessing = false;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    public event EventHandler<TransactionDto>? CheckoutCompleted;

    public CheckoutViewModel(ITransactionService transactionService, IPrinterService printerService)
    {
        _transactionService = transactionService;
        _printerService = printerService;
    }

    partial void OnAmountPaidChanged(decimal value)
    {
        if (SelectedPaymentMethod == PaymentMethod.Cash)
        {
            Change = value - TotalAmount;
        }
    }

    [RelayCommand]
    public async Task ProcessPaymentAsync(List<CartItemViewModel> cartItems)
    {
        if (cartItems == null || cartItems.Count == 0)
        {
            ErrorMessage = "Keranjang kosong";
            return;
        }

        if (SelectedPaymentMethod == PaymentMethod.Cash && AmountPaid < TotalAmount)
        {
            ErrorMessage = "Nominal pembayaran kurang";
            return;
        }

        IsProcessing = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var request = new CreateTransactionRequest(
                cartItems.Select(i => new TransactionItemRequest(i.ProductId, i.Quantity)).ToList(),
                SelectedPaymentMethod,
                AmountPaid);

            var result = await _transactionService.ProcessSaleAsync(request);

            if (result.Success && result.Transaction != null)
            {
                SuccessMessage = $"Transaksi berhasil. Invoice: {result.Transaction.InvoiceNumber}";
                
                // Try to print receipt
                try
                {
                    await _printerService.PrintReceiptAsync(result.Transaction);
                }
                catch
                {
                    // Printer error is not critical
                }

                CheckoutCompleted?.Invoke(this, result.Transaction);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Transaksi gagal";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Terjadi kesalahan: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
