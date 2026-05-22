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

        When(x => !string.IsNullOrEmpty(x.Cpf), () =>
        {
            RuleFor(x => x.Cpf!)
                .Must(BeValidCpf).WithMessage("CPF inválido.");
        });

        When(x => x.Endereco is not null, () =>
        {
            RuleFor(x => x.Endereco!)
                .SetValidator(new EnderecoInfoValidator());
        });
    }

    private static bool BeValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var numeros = cpf.Where(char.IsDigit).ToArray();
        if (numeros.Length != 11) return false;
        if (numeros.Distinct().Count() == 1) return false;

        var digito1 = CalcularDigito(numeros, 9, 10);
        if (digito1 != numeros[9] - '0') return false;

        var digito2 = CalcularDigito(numeros, 10, 11);
        return digito2 == numeros[10] - '0';
    }

    private static int CalcularDigito(char[] numeros, int tamanho, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < tamanho; i++)
            soma += (numeros[i] - '0') * (pesoInicial - i);
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
