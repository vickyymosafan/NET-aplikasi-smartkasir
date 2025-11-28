using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartKasir.Client.Services;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Client.ViewModels;

/// <summary>
/// ViewModel untuk DashboardView
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string currentUserName = string.Empty;

    [ObservableProperty]
    private bool isOnline = false;

    [ObservableProperty]
    private int pendingTransactionCount = 0;

    public ProductGridViewModel ProductGridViewModel { get; }
    public CartViewModel CartViewModel { get; }
    public CheckoutViewModel CheckoutViewModel { get; }

    public DashboardViewModel(
        IAuthService authService,
        INavigationService navigationService,
        ProductGridViewModel productGridViewModel,
        CartViewModel cartViewModel,
        CheckoutViewModel checkoutViewModel)
    {
        _authService = authService;
        _navigationService = navigationService;
        ProductGridViewModel = productGridViewModel;
        CartViewModel = cartViewModel;
        CheckoutViewModel = checkoutViewModel;

        CurrentUserName = authService.CurrentUser?.Username ?? "Unknown";
        
        // Subscribe to product selection
        ProductGridViewModel.ProductSelected += OnProductSelected;
        CheckoutViewModel.CheckoutCompleted += OnCheckoutCompleted;
    }

    private void OnProductSelected(object? sender, ProductDto product)
    {
        if (product != null && product.StockQty > 0)
        {
            CartViewModel.AddItem(product);
        }
    }

    private void OnCheckoutCompleted(object? sender, TransactionDto transaction)
    {
        CartViewModel.Clear();
        CheckoutViewModel.AmountPaid = 0;
        CheckoutViewModel.Change = 0;
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        _navigationService.NavigateTo("Login");
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        await ProductGridViewModel.LoadProductsCommand.ExecuteAsync(null);
    }
}
