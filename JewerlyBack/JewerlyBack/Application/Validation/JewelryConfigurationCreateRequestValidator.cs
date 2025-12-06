using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса создания конфигурации украшения
/// </summary>
public class JewelryConfigurationCreateRequestValidator : AbstractValidator<JewelryConfigurationCreateRequest>
{
    public JewelryConfigurationCreateRequestValidator()
    {
        // BaseModelId валидация
        RuleFor(x => x.BaseModelId)
            .NotEmpty()
            .WithMessage("BaseModelId is required and must not be empty");

        // MaterialId валидация
        RuleFor(x => x.MaterialId)
            .GreaterThan(0)
            .WithMessage("MaterialId must be greater than 0");

        // Name (опционально)
        RuleFor(x => x.Name)
            .MaximumLength(500)
            .WithMessage("Name must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        // ConfigJson (опционально, но если передан - проверяем длину)
        // Разрешаем пустые JSON объекты "{}" и null
        RuleFor(x => x.ConfigJson)
            .MaximumLength(10000)
            .WithMessage("ConfigJson must not exceed 10000 characters")
            .When(x => !string.IsNullOrEmpty(x.ConfigJson));
    }
}
