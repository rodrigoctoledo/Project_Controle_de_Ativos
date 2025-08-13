using FluentValidation;
using AssetControl.Application.DTOs;

namespace AssetControl.Application.Validators;

public class AssetUpdateDtoValidator : AbstractValidator<AssetUpdateDto>
{
    public AssetUpdateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);
    }
}
