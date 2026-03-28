using CafeSystem.Application.DTOs;
using CafeSystem.Application.Validation;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .Must(PasswordValidationHelper.IsProvided)
                .WithMessage("O campo senha é obrigatório")
                .Must(PasswordValidationHelper.HasMinimumLength)
                .WithMessage("Senha deve conter 5 ou mais caracteres")
                .Must(PasswordValidationHelper.HasMaximumLength)
                .WithMessage("Senha deve conter no máximo 20 caracteres");
        }
    }
}
