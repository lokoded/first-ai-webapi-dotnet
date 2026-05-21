using FirstWebApi.Application.DTOs.Request;
using FluentValidation;

namespace FirstWebApi.Application.Validators;

public class ComicTypeRequestValidator : AbstractValidator<ComicTypeRequest>
{
    public ComicTypeRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(50);
    }
}
