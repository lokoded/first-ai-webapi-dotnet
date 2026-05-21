using FirstWebApi.Application.DTOs;
using FluentValidation;

namespace FirstWebApi.Application.Validators;

public class EnderecoInfoValidator : AbstractValidator<EnderecoInfo>
{
    public EnderecoInfoValidator()
    {
        RuleFor(x => x.Logradouro)
            .NotEmpty().WithMessage("Logradouro é obrigatório.")
            .MaximumLength(200);

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Número é obrigatório.")
            .MaximumLength(20);

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro é obrigatório.")
            .MaximumLength(100);

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória.")
            .MaximumLength(100);

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório.")
            .MaximumLength(50);

        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório.")
            .MaximumLength(10);

        RuleFor(x => x.Pais)
            .NotEmpty().WithMessage("País é obrigatório.")
            .MaximumLength(50);

        RuleFor(x => x.Complemento)
            .MaximumLength(200);
    }
}
