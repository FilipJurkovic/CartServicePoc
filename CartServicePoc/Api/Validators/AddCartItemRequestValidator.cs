using CartServicePoc.Api.DTOs;
using FluentValidation;

namespace CartServicePoc.Api.Validators;

public class AddCartItemRequestValidator : AbstractValidator<AddCartItemRequest>
{
    public AddCartItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId je obavezan.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("ProductName je obavezan.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Cijena mora biti veća od 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Količina mora biti veća od 0.");
    }
}
