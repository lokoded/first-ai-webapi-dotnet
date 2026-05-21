using FirstWebApi.Application.DTOs.Request;
using FluentValidation;

namespace FirstWebApi.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("UserName é obrigatório.")
            .MinimumLength(3).WithMessage("UserName deve ter no mínimo 3 caracteres.")
            .MaximumLength(50).WithMessage("UserName deve ter no máximo 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("UserName deve conter apenas letras, números e underscore.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula.")
            .Matches(@"[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula.")
            .Matches(@"\d").WithMessage("Senha deve conter pelo menos um número.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Cpf) || !string.IsNullOrEmpty(x.Rg))
            .WithMessage("CPF ou RG deve ser informado.");

        When(x => x.Endereco is not null, () =>
        {
            RuleFor(x => x.Endereco!)
                .SetValidator(new EnderecoInfoValidator());
        });
    }
}
