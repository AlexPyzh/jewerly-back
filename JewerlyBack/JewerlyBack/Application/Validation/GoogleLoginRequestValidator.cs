using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса входа через Google Sign-In
/// </summary>
public class GoogleLoginRequestValidator : AbstractValidator<GoogleLoginRequest>
{
    public GoogleLoginRequestValidator()
    {
        // IdToken валидация
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .WithMessage("IdToken is required")
            .MinimumLength(50)
            .WithMessage("IdToken appears to be invalid (too short)");
    }
}
