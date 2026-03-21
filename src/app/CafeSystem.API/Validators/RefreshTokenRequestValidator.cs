using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("É obrigatório informar um token");

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("É obrigatório informar um refresh token");
        }
    }
}
