using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("O campo nome é obrigatório")
                .Must(x => x.Trim().Length >= 5)
                .WithMessage("O campo nome deve conter 5 ou mais caracteres")
                .Must(x => x.Trim().Length <= 250)
                .WithMessage("O campo nome deve conter no máximo 250 caracteres.");

            RuleFor(x => x.BirthDate)
                .Must(BirthDateValidationHelper.BeValidBirthDate)
                .WithMessage("Data de nascimento inválida.")
                .When(x => !string.IsNullOrWhiteSpace(x.BirthDate));
        }
    }
}
