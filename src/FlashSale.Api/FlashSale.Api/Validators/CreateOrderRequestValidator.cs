using FlashSale.Api.DTOs;
using FluentValidation;

namespace FlashSale.Api.Validators;

/// <summary>
/// Validador para CreateOrderRequest.
/// Segue regras de SEC-003: Validar TODA entrada do usuário.
/// </summary>
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId é obrigatório");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("IdempotencyKey é obrigatória")
            .MaximumLength(100)
            .WithMessage("IdempotencyKey deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("IdempotencyKey contém caracteres inválidos");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Pedido deve ter pelo menos 1 item")
            .Must(items => items.Count <= 10)
            .WithMessage("Máximo de 10 itens por pedido");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());

        RuleFor(x => x.UtmSource)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.UtmSource));

        RuleFor(x => x.UtmMedium)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.UtmMedium));

        RuleFor(x => x.UtmCampaign)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.UtmCampaign));
    }
}

/// <summary>
/// Validador para OrderItemRequest.
/// </summary>
public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("ProductId é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Máximo de 10 unidades por item");
    }
}
