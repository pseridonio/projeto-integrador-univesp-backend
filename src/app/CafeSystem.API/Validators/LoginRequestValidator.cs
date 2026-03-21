using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("É obrigatório informar um e-mail")
                .EmailAddress()
                .WithMessage("O formato de e-mail não é valido");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("É obrigatório informar uma senha");
        }
    }
}
