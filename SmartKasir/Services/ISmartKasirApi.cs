using System.Net.Http;
using Refit;
using SmartKasir.Application.DTOs;
using SmartKasir.Core.Enums;

namespace SmartKasir.Client.Services;

/// <summary>
/// Refit API client interface untuk SmartKasir Server
/// </summary>
[Headers("Authorization: Bearer")]
public interface ISmartKasirApi
{
    #region Authentication

    /// <summary>
    /// Login dengan username dan password
    /// </summary>
    [Post("/api/v1/auth/login")]
    Task<AuthResponse> LoginAsync([Body] LoginRequest request);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [Post("/api/v1/auth/refresh")]
    Task<AuthResponse> RefreshTokenAsync([Body] RefreshTokenRequest request);

    /// <summary>
    /// Logout (invalidate token)
    /// </summary>
    [Post("/api/v1/auth/logout")]
    Task LogoutAsync();

    #endregion

    #region Products

    /// <summary>
    /// Get semua produk dengan pagination
    /// </summary>
    [Get("/api/v1/products")]
    Task<PagedResult<ProductDto>> GetProductsAsync(
        [Query] int page = 1,
        [Query] int pageSize = 50);

    /// <summary>
    /// Get produk berdasarkan barcode
    /// </summary>
    [Get("/api/v1/products/{barcode}")]
    Task<ProductDto> GetProductByBarcodeAsync(string barcode);

    /// <summary>
    /// Search produk berdasarkan keyword
    /// </summary>
    [Get("/api/v1/products/search")]
    Task<List<ProductDto>> SearchProductsAsync([Query] string keyword);

    /// <summary>
    /// Create produk baru (Admin only)
    /// </summary>
    [Post("/api/v1/products")]
    Task<ProductDto> CreateProductAsync([Body] CreateProductRequest request);

    /// <summary>
    /// Update produk (Admin only)
    /// </summary>
    [Put("/api/v1/products/{id}")]
    Task<ProductDto> UpdateProductAsync(Guid id, [Body] UpdateProductRequest request);

    /// <summary>
    /// Delete produk (Admin only)
    /// </summary>
    [Delete("/api/v1/products/{id}")]
    Task DeleteProductAsync(Guid id);

    #endregion

    #region Transactions

    /// <summary>
    /// Create transaksi baru
    /// </summary>
    [Post("/api/v1/transactions")]
    Task<TransactionDto> CreateTransactionAsync([Body] CreateTransactionRequest request);

    /// <summary>
    /// Get transaksi berdasarkan ID
    /// </summary>
    [Get("/api/v1/transactions/{id}")]
    Task<TransactionDto> GetTransactionAsync(Guid id);

    /// <summary>
    /// Get semua transaksi dengan filter (Admin only)
    /// </summary>
    [Get("/api/v1/transactions")]
    Task<PagedResult<TransactionDto>> GetTransactionsAsync(
        [Query] DateTime? startDate = null,
        [Query] DateTime? endDate = null,
        [Query] int page = 1,
        [Query] int pageSize = 20);

    #endregion

    #region Categories

    /// <summary>
    /// Get semua kategori
    /// </summary>
    [Get("/api/v1/categories")]
    Task<List<CategoryDto>> GetCategoriesAsync();

    /// <summary>
    /// Create kategori baru (Admin only)
    /// </summary>
    [Post("/api/v1/categories")]
    Task<CategoryDto> CreateCategoryAsync([Body] CreateCategoryRequest request);

    /// <summary>
    /// Update kategori (Admin only)
    /// </summary>
    [Put("/api/v1/categories/{id}")]
    Task<CategoryDto> UpdateCategoryAsync(int id, [Body] UpdateCategoryRequest request);

    /// <summary>
    /// Delete kategori (Admin only)
    /// </summary>
    [Delete("/api/v1/categories/{id}")]
    Task DeleteCategoryAsync(int id);

    #endregion

    #region Users

    /// <summary>
    /// Get semua user (Admin only)
    /// </summary>
    [Get("/api/v1/users")]
    Task<List<UserDto>> GetUsersAsync();

    /// <summary>
    /// Create user baru (Admin only)
    /// </summary>
    [Post("/api/v1/users")]
    Task<UserDto> CreateUserAsync([Body] CreateUserRequest request);

    /// <summary>
    /// Update user (Admin only)
    /// </summary>
    [Put("/api/v1/users/{id}")]
    Task<UserDto> UpdateUserAsync(Guid id, [Body] UpdateUserRequest request);

    /// <summary>
    /// Reset password user (Admin only)
    /// </summary>
    [Post("/api/v1/users/{id}/reset-password")]
    Task<UserDto> ResetPasswordAsync(Guid id);

    #endregion

    #region Reports

    /// <summary>
    /// Get laporan penjualan harian
    /// </summary>
    [Get("/api/v1/reports/daily")]
    Task<DailySalesReportDto> GetDailySalesReportAsync(
        [Query] DateTime date);

    /// <summary>
    /// Get laporan penjualan per produk
    /// </summary>
    [Get("/api/v1/reports/products")]
    Task<ProductSalesReportDto> GetProductSalesReportAsync(
        [Query] DateTime startDate,
        [Query] DateTime endDate);

    /// <summary>
    /// Export laporan ke PDF
    /// </summary>
    [Get("/api/v1/reports/export/pdf")]
    Task<HttpContent> ExportReportToPdfAsync(
        [Query] DateTime startDate,
        [Query] DateTime endDate);

    /// <summary>
    /// Export laporan ke Excel
    /// </summary>
    [Get("/api/v1/reports/export/excel")]
    Task<HttpContent> ExportReportToExcelAsync(
        [Query] DateTime startDate,
        [Query] DateTime endDate);

    #endregion
}

/// <summary>
/// Paged result wrapper
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// DTO untuk kategori
/// </summary>
public record CategoryDto(
    int Id,
    string Name);

/// <summary>
/// Request untuk create kategori
/// </summary>
public record CreateCategoryRequest(string Name);

/// <summary>
/// Request untuk update kategori
/// </summary>
public record UpdateCategoryRequest(string? Name);

// UserDto is defined in IAuthService.cs

/// <summary>
/// Request untuk create user
/// </summary>
public record CreateUserRequest(
    string Username,
    string Password,
    UserRole Role);

/// <summary>
/// Request untuk update user
/// </summary>
public record UpdateUserRequest(
    string? Username,
    UserRole? Role,
    bool? IsActive);

/// <summary>
/// DTO untuk laporan penjualan harian
/// </summary>
public record DailySalesReportDto(
    DateTime Date,
    int TransactionCount,
    decimal TotalSales,
    decimal TotalTax,
    List<PaymentMethodSummary> PaymentMethods);

/// <summary>
/// Summary per metode pembayaran
/// </summary>
public record PaymentMethodSummary(
    PaymentMethod Method,
    int Count,
    decimal Amount);

/// <summary>
/// DTO untuk laporan penjualan per produk
/// </summary>
public record ProductSalesReportDto(
    DateTime StartDate,
    DateTime EndDate,
    List<ProductSalesDetail> Products);

/// <summary>
/// Detail penjualan per produk
/// </summary>
public record ProductSalesDetail(
    Guid ProductId,
    string ProductName,
    string CategoryName,
    int QuantitySold,
    decimal Revenue);
