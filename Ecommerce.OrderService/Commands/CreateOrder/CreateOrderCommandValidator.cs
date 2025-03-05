using FluentValidation;

namespace Ecommerce.OrderService.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(createordercommand 
            => createordercommand.CustomerName).NotEmpty().MaximumLength(30)
            .WithMessage("Customer name is invalid!");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000.");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("ProductId must be a valid positive number.");
    }
}
