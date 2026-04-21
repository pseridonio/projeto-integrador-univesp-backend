using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Barcode)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Código de barras é obrigatório");

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Descrição é obrigatória")
                .Must(x => x.Trim().Length >= 3)
                .WithMessage("Descrição deve conter entre 3 e 250 caracteres")
                .Must(x => x.Trim().Length <= 250)
                .WithMessage("Descrição deve conter entre 3 e 250 caracteres");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Valor unitário deve ser maior ou igual a 0");

            RuleFor(x => x.Categories)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("O produto deve ter ao menos 1 categoria")
                .Must(x => x.Count > 0)
                .WithMessage("O produto deve ter ao menos 1 categoria");

            RuleForEach(x => x.Categories)
                .GreaterThan(0)
                .WithMessage("Categoria inválida");
        }
    }
}
