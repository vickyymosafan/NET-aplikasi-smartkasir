using FluentValidation;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Barcode)
            .NotEmpty().WithMessage("Barcode tidak boleh kosong")
            .MaximumLength(50).WithMessage("Barcode maksimal 50 karakter");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama produk tidak boleh kosong")
            .MaximumLength(200).WithMessage("Nama produk maksimal 200 karakter");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Harga harus lebih dari 0");

        RuleFor(x => x.StockQty)
            .GreaterThanOrEqualTo(0).WithMessage("Stok tidak boleh negatif");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kategori harus dipilih");
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Nama produk maksimal 200 karakter")
            .When(x => x.Name != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Harga harus lebih dari 0")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.StockQty)
            .GreaterThanOrEqualTo(0).WithMessage("Stok tidak boleh negatif")
            .When(x => x.StockQty.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kategori tidak valid")
            .When(x => x.CategoryId.HasValue);
    }
}
