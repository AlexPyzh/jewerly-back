using FluentValidation;
using JewerlyBack.Dto;
using System.Text.RegularExpressions;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса регистрации нового пользователя
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // Email валидация
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        // Пароль валидация
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .WithMessage("Password must not exceed 128 characters")
            .Must(ContainDigit)
            .WithMessage("Password must contain at least one digit")
            .Must(ContainLetter)
            .WithMessage("Password must contain at least one letter");

        // Имя (опционально)
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }

    /// <summary>
    /// Проверяет наличие хотя бы одной цифры в пароле
    /// </summary>
    private bool ContainDigit(string password)
    {
        return Regex.IsMatch(password, @"\d");
    }

    /// <summary>
    /// Проверяет наличие хотя бы одной буквы в пароле
    /// </summary>
    private bool ContainLetter(string password)
    {
        return Regex.IsMatch(password, @"[a-zA-Z]");
    }
}
