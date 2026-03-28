using CafeSystem.Application.DTOs;
using CafeSystem.Application.Validation;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .Must(CategoryValidationHelper.IsValidDescription)
                .WithMessage("Descrição deve conter entre 5 e 50 caracteres e usar apenas letras e números");
        }
    }
}
