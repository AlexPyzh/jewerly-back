using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса обновления конфигурации украшения
/// </summary>
public class JewelryConfigurationUpdateRequestValidator : AbstractValidator<JewelryConfigurationUpdateRequest>
{
    public JewelryConfigurationUpdateRequestValidator()
    {
        // MaterialId (опционально, но если передан - проверяем)
        RuleFor(x => x.MaterialId)
            .GreaterThan(0)
            .WithMessage("MaterialId must be greater than 0")
            .When(x => x.MaterialId.HasValue);

        // Name (опционально)
        RuleFor(x => x.Name)
            .MaximumLength(500)
            .WithMessage("Name must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        // ConfigJson (опционально, но если передан - проверяем)
        RuleFor(x => x.ConfigJson)
            .NotEmpty()
            .WithMessage("ConfigJson must not be empty if provided")
            .MaximumLength(10000)
            .WithMessage("ConfigJson must not exceed 10000 characters")
            .When(x => x.ConfigJson != null);

        // Status (опционально)
        RuleFor(x => x.Status)
            .MaximumLength(50)
            .WithMessage("Status must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Status));

        // Stones коллекция (если передана)
        RuleForEach(x => x.Stones)
            .SetValidator(new ConfigurationStoneDtoValidator())
            .When(x => x.Stones != null && x.Stones.Any());

        // Engravings коллекция (если передана)
        RuleForEach(x => x.Engravings)
            .SetValidator(new ConfigurationEngravingDtoValidator())
            .When(x => x.Engravings != null && x.Engravings.Any());
    }
}

/// <summary>
/// Валидатор для элемента камня в конфигурации
/// </summary>
public class ConfigurationStoneDtoValidator : AbstractValidator<ConfigurationStoneDto>
{
    public ConfigurationStoneDtoValidator()
    {
        RuleFor(x => x.StoneTypeId)
            .GreaterThan(0)
            .WithMessage("StoneTypeId must be greater than 0");

        RuleFor(x => x.StoneTypeName)
            .NotEmpty()
            .WithMessage("StoneTypeName is required")
            .MaximumLength(200)
            .WithMessage("StoneTypeName must not exceed 200 characters");

        RuleFor(x => x.PositionIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("PositionIndex must be greater than or equal to 0");

        RuleFor(x => x.CaratWeight)
            .GreaterThan(0)
            .WithMessage("CaratWeight must be greater than 0")
            .When(x => x.CaratWeight.HasValue);

        RuleFor(x => x.SizeMm)
            .GreaterThan(0)
            .WithMessage("SizeMm must be greater than 0")
            .When(x => x.SizeMm.HasValue);

        RuleFor(x => x.Count)
            .GreaterThan(0)
            .WithMessage("Count must be greater than 0");
    }
}

/// <summary>
/// Валидатор для элемента гравировки в конфигурации
/// </summary>
public class ConfigurationEngravingDtoValidator : AbstractValidator<ConfigurationEngravingDto>
{
    public ConfigurationEngravingDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Engraving text is required")
            .MaximumLength(500)
            .WithMessage("Engraving text must not exceed 500 characters");

        RuleFor(x => x.FontName)
            .MaximumLength(100)
            .WithMessage("FontName must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FontName));

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .MaximumLength(100)
            .WithMessage("Location must not exceed 100 characters");

        RuleFor(x => x.SizeMm)
            .GreaterThan(0)
            .WithMessage("SizeMm must be greater than 0")
            .When(x => x.SizeMm.HasValue);
    }
}
