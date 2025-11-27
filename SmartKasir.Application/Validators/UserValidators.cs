using FluentValidation;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username tidak boleh kosong")
            .MinimumLength(3).WithMessage("Username minimal 3 karakter")
            .MaximumLength(50).WithMessage("Username maksimal 50 karakter")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username hanya boleh huruf, angka, dan underscore");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password tidak boleh kosong")
            .MinimumLength(6).WithMessage("Password minimal 6 karakter");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role tidak valid");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password baru tidak boleh kosong")
            .MinimumLength(6).WithMessage("Password minimal 6 karakter");
    }
}
