using FluentValidation;
using AssetControl.Application.DTOs;

namespace AssetControl.Application.Validators;

public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutDtoValidator()
    {
        RuleFor(x => x.TakenBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Note).MaximumLength(200);
    }
}
