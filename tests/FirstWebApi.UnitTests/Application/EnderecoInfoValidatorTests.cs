using FirstWebApi.Application.DTOs;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class EnderecoInfoValidatorTests
{
    private readonly EnderecoInfoValidator _validator = new();

    private static EnderecoInfo ValidEndereco => new()
    {
        Logradouro = "Rua ABC",
        Numero = "123",
        Bairro = "Centro",
        Cidade = "São Paulo",
        Estado = "SP",
        Cep = "01001-000",
        Pais = "Brasil"
    };

    [Fact]
    public void Validate_WithAllValidFields_ShouldBeValid()
    {
        var result = _validator.Validate(ValidEndereco);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyLogradouro_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Logradouro");
    }

    [Fact]
    public void Validate_WithEmptyNumero_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Numero");
    }

    [Fact]
    public void Validate_WithEmptyBairro_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Bairro");
    }

    [Fact]
    public void Validate_WithEmptyCidade_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cidade");
    }

    [Fact]
    public void Validate_WithEmptyEstado_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Estado");
    }

    [Fact]
    public void Validate_WithEmptyCep_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cep");
    }

    [Fact]
    public void Validate_WithEmptyPais_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = "Rua ABC",
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = ""
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Pais");
    }

    [Fact]
    public void Validate_WithLogradouroExceedingMaxLength_ShouldHaveError()
    {
        var endereco = new EnderecoInfo
        {
            Logradouro = new string('A', 201),
            Numero = "123",
            Bairro = "Centro",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01001-000",
            Pais = "Brasil"
        };

        var result = _validator.Validate(endereco);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Logradouro");
    }
}
