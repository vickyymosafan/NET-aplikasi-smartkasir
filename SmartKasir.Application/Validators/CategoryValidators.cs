using FluentValidation;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Validators;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama kategori tidak boleh kosong")
            .MaximumLength(100).WithMessage("Nama kategori maksimal 100 karakter");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama kategori tidak boleh kosong")
            .MaximumLength(100).WithMessage("Nama kategori maksimal 100 karakter");
    }
}
