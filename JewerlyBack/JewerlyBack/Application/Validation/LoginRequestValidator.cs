using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса входа через email/password
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Email валидация
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        // Пароль валидация
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}
