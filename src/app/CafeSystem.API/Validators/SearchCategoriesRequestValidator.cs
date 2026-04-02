using CafeSystem.Application.DTOs;
using FluentValidation;

namespace CafeSystem.API.Validators
{
    /// <summary>
    /// Validador para requisição de busca de categorias.
    /// </summary>
    public class SearchCategoriesRequestValidator : AbstractValidator<SearchCategoriesRequest>
    {
        public SearchCategoriesRequestValidator()
        {
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Descrição é obrigatória para realizar a busca");
        }
    }
}
