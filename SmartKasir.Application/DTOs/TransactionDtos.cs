using SmartKasir.Core.Enums;

namespace SmartKasir.Application.DTOs;

/// <summary>
/// DTO untuk menampilkan data transaksi
/// </summary>
public class TransactionDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TransactionItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO untuk item transaksi
/// </summary>
public class TransactionItemDto
{
    public long Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PriceAtMoment { get; set; }
    public decimal Subtotal { get; set; }
}

/// <summary>
/// Request untuk membuat transaksi baru
/// </summary>
public class CreateTransactionRequest
{
    public List<TransactionItemRequest> Items { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
}

/// <summary>
/// Request untuk item transaksi
/// </summary>
public class TransactionItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>
/// Result dari proses transaksi
/// </summary>
public class TransactionResult
{
    public bool Success { get; set; }
    public TransactionDto? Transaction { get; set; }
    public decimal Change { get; set; }
    public string? ErrorMessage { get; set; }

    public TransactionResult() { }
    public TransactionResult(bool success, TransactionDto? transaction, decimal change, string? errorMessage)
    {
        Success = success;
        Transaction = transaction;
        Change = change;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Filter untuk query transaksi
/// </summary>
public class TransactionFilterParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CashierId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
