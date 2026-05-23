using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WithAllValidFields_ShouldBeValid()
    {
        var request = new RegisterRequest
        {
            Nome = "João Silva",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyNome_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_WithShortNome_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "Jo",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome"
            && e.ErrorMessage.Contains("3 caracteres"));
    }

    [Fact]
    public void Validate_WithEmptyUserName_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserName");
    }

    [Fact]
    public void Validate_WithInvalidUserNameChars_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joão@#$",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserName"
            && e.ErrorMessage.Contains("letras, números e underscore"));
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "invalido",
            Senha = "SenhaForte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email"
            && e.ErrorMessage.Contains("Email inválido"));
    }

    [Fact]
    public void Validate_WithEmptySenha_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Validate_WithSenhaNoUppercase_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "senhaforte123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha"
            && e.ErrorMessage.Contains("maiúscula"));
    }

    [Fact]
    public void Validate_WithSenhaNoLowercase_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SENHAFORTE123",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha"
            && e.ErrorMessage.Contains("minúscula"));
    }

    [Fact]
    public void Validate_WithSenhaNoNumber_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte",
            Cpf = "52998224725"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha"
            && e.ErrorMessage.Contains("número"));
    }

    [Fact]
    public void Validate_WithBothCpfAndRgEmpty_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("CPF ou RG deve ser informado"));
    }

    [Fact]
    public void Validate_WithInvalidCpfLength_ShouldHaveError()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cpf"
            && e.ErrorMessage.Contains("CPF inválido"));
    }

    [Fact]
    public void Validate_WithRgOnly_ShouldBeValid()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Rg = "12.345.678-9"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEndereco_ShouldValidateEnderecoInfo()
    {
        var request = new RegisterRequest
        {
            Nome = "João",
            UserName = "joao_silva",
            Email = "joao@email.com",
            Senha = "SenhaForte123",
            Cpf = "52998224725",
            Endereco = new EnderecoInfo()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.StartsWith("Endereco."));
    }
}
