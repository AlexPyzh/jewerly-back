using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса входа через Apple Sign-In
/// </summary>
public class AppleLoginRequestValidator : AbstractValidator<AppleLoginRequest>
{
    public AppleLoginRequestValidator()
    {
        // IdToken валидация
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("IdToken is required")
            .MinimumLength(50)
            .WithMessage("IdToken appears to be invalid (too short)");

        // FullName (опционально)
        RuleFor(x => x.FullName)
            .MaximumLength(200)
            .WithMessage("FullName must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));
    }
}
