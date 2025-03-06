using FluentValidation;

namespace Ecommerce.ProductService.Query;

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
           .GreaterThan(0).WithMessage("Id must be a valid positive number.");
    }
}
