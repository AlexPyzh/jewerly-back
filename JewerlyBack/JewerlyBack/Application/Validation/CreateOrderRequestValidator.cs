using FluentValidation;
using JewerlyBack.Dto;

namespace JewerlyBack.Application.Validation;

/// <summary>
/// Валидатор для запроса создания заказа
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        // Items валидация - должен быть хотя бы один элемент
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        // Валидация каждого элемента заказа
        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemRequestValidator());

        // ContactName (опционально)
        RuleFor(x => x.ContactName)
            .MaximumLength(200)
            .WithMessage("ContactName must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactName));

        // ContactEmail (опционально, но если передан - проверяем формат)
        RuleFor(x => x.ContactEmail)
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("ContactEmail must not exceed 256 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        // ContactPhone (опционально)
        RuleFor(x => x.ContactPhone)
            .MaximumLength(50)
            .WithMessage("ContactPhone must not exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.ContactPhone));

        // DeliveryAddress (опционально)
        RuleFor(x => x.DeliveryAddress)
            .MaximumLength(1000)
            .WithMessage("DeliveryAddress must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.DeliveryAddress));

        // Notes (опционально)
        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

/// <summary>
/// Валидатор для элемента заказа
/// </summary>
public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemRequestValidator()
    {
        // ConfigurationId валидация
        RuleFor(x => x.ConfigurationId)
            .NotEmpty()
            .WithMessage("ConfigurationId is required and must not be empty");

        // Quantity валидация
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity must not exceed 1000");
    }
}
