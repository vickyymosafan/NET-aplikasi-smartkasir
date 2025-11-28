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

    public TransactionDto() { }
    public TransactionDto(Guid id, string invoiceNumber, Guid cashierId, string cashierName, decimal totalAmount, decimal taxAmount, PaymentMethod paymentMethod, DateTime createdAt, List<TransactionItemDto> items)
    {
        Id = id;
        InvoiceNumber = invoiceNumber;
        CashierId = cashierId;
        CashierName = cashierName;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        PaymentMethod = paymentMethod;
        CreatedAt = createdAt;
        Items = items;
    }
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

    public TransactionItemDto() { }
    public TransactionItemDto(long id, Guid productId, string productName, int quantity, decimal priceAtMoment, decimal subtotal)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        PriceAtMoment = priceAtMoment;
        Subtotal = subtotal;
    }
}

/// <summary>
/// Request untuk membuat transaksi baru
/// </summary>
public class CreateTransactionRequest
{
    public List<TransactionItemRequest> Items { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }

    public CreateTransactionRequest() { }
    public CreateTransactionRequest(List<TransactionItemRequest> items, PaymentMethod paymentMethod, decimal amountPaid)
    {
        Items = items;
        PaymentMethod = paymentMethod;
        AmountPaid = amountPaid;
    }
}

/// <summary>
/// Request untuk item transaksi
/// </summary>
public class TransactionItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    public TransactionItemRequest() { }
    public TransactionItemRequest(Guid productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
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
