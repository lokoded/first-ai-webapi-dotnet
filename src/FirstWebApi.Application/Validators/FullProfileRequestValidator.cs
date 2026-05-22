using FirstWebApi.Application.DTOs.Request;
using FluentValidation;

namespace FirstWebApi.Application.Validators;

public class FullProfileRequestValidator : AbstractValidator<FullProfileRequest>
{
    public FullProfileRequestValidator()
    {
        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
