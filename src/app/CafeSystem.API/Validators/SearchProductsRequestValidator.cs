using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    public class SearchProductsRequestValidator : AbstractValidator<SearchProductsRequest>
    {
        public SearchProductsRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue)
                .WithMessage("Id deve ser maior que 0");

            RuleFor(x => x.Description)
                .MaximumLength(250)
                .When(x => x.Description is not null)
                .WithMessage("Descrição deve ter no máximo 250 caracteres");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .When(x => x.CategoryId.HasValue)
                .WithMessage("CategoryId deve ser maior que 0");

            RuleFor(x => x.Barcode)
                .MaximumLength(50)
                .When(x => x.Barcode is not null)
                .WithMessage("Barcode deve ter no máximo 50 caracteres");

            RuleFor(x => x.Sort)
                .Must(IsValidSort)
                .When(x => x.Sort is not null)
                .WithMessage("Parâmetro sort inválido");
        }

        private static bool IsValidSort(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
            {
                return false;
            }

            string normalizedSort = sort.StartsWith('-') ? sort[1..] : sort;
            return normalizedSort is "id" or "barcode" or "description" or "unitPrice" or "createdAt" or "updatedAt";
        }
    }
}
