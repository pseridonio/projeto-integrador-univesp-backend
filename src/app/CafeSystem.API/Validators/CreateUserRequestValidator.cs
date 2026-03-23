using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Campo nome é obrigatório")
                .Must(x => x.Trim().Length >= 5)
                .WithMessage("Nome deve conter 5 ou mais caracteres")
                .Must(x => x.Trim().Length <= 250)
                .WithMessage("Nome deve conter no máximo 250 caracteres");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Campo e-mail é obrigatório")
                .EmailAddress()
                .WithMessage("Campo e-mail está em um formato inválido");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("O campo senha é obrigatório")
                .Must(x => x.Trim().Length >= 5)
                .WithMessage("Senha deve conter 5 ou mais caracteres")
                .Must(x => x.Trim().Length <= 20)
                .WithMessage("Senha deve conter no máximo 20 caracteres");

            RuleFor(x => x.BirthDate)
                .Must(BirthDateValidationHelper.BeValidBirthDate)
                .WithMessage("Data de nascimento inválida")
                .When(x => !string.IsNullOrWhiteSpace(x.BirthDate));
        }
    }
}
