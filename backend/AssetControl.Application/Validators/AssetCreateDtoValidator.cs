using FluentValidation;
using AssetControl.Application.DTOs;

namespace AssetControl.Application.Validators;

public class AssetCreateDtoValidator : AbstractValidator<AssetCreateDto>
{
    public AssetCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MinimumLength(2).MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty().MinimumLength(2).MaximumLength(50);
    }
}
