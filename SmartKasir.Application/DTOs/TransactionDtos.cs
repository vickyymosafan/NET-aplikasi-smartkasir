using SmartKasir.Core.Enums;

namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data transaksi
/// </summary>
public record TransactionDto(
    Guid Id,
    string InvoiceNumber,
    Guid CashierId,
    string CashierName,
    decimal TotalAmount,
    decimal TaxAmount,
    PaymentMethod PaymentMethod,
    DateTime CreatedAt,
    List<TransactionItemDto> Items);

/// <summary>
/// DTO untuk item transaksi
/// </summary>
public record TransactionItemDto(
    long Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal PriceAtMoment,
    decimal Subtotal);

/// <summary>
/// Request untuk membuat transaksi baru
/// </summary>
public record CreateTransactionRequest(
    List<TransactionItemRequest> Items,
    PaymentMethod PaymentMethod,
    decimal AmountPaid);

/// <summary>
/// Request untuk item transaksi
/// </summary>
public record TransactionItemRequest(Guid ProductId, int Quantity);

/// <summary>
/// Result dari proses transaksi
/// </summary>
public record TransactionResult(
    bool Success,
    TransactionDto? Transaction,
    decimal Change,
    string? ErrorMessage);

/// <summary>
/// Filter untuk query transaksi
/// </summary>
public record TransactionFilterParams(
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? CashierId,
    int Page = 1,
    int PageSize = 20);
