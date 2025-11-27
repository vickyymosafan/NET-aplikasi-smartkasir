using FluentValidation;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Validators;

public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Transaksi harus memiliki minimal 1 item");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Metode pembayaran tidak valid");

        RuleFor(x => x.AmountPaid)
            .GreaterThanOrEqualTo(0).WithMessage("Jumlah pembayaran tidak valid");

        RuleForEach(x => x.Items).SetValidator(new TransactionItemRequestValidator());
    }
}

public class TransactionItemRequestValidator : AbstractValidator<TransactionItemRequest>
{
    public TransactionItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID tidak boleh kosong");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Kuantitas harus lebih dari 0");
    }
}
