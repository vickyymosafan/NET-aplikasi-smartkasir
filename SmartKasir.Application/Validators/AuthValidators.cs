using FluentValidation;
using SmartKasir.Application.DTOs;

namespace SmartKasir.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username tidak boleh kosong")
            .MinimumLength(3).WithMessage("Username minimal 3 karakter");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password tidak boleh kosong")
            .MinimumLength(6).WithMessage("Password minimal 6 karakter");
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token tidak boleh kosong");
    }
}
