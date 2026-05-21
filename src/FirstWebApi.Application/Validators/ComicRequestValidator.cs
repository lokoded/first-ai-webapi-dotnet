using FirstWebApi.Application.DTOs.Request;
using FluentValidation;

namespace FirstWebApi.Application.Validators;

public class ComicRequestValidator : AbstractValidator<ComicRequest>
{
    public ComicRequestValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(300);

        RuleFor(x => x.WebUrl)
            .NotEmpty().WithMessage("WebUrl é obrigatório.")
            .MaximumLength(2048)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("WebUrl deve ser uma URL válida.");

        RuleFor(x => x.ComicTypeId)
            .NotEmpty().WithMessage("Tipo de comic é obrigatório.");
    }
}
