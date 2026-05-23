using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_WithAllValidFields_ShouldBeValid()
    {
        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = "SenhaForte123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveError()
    {
        var request = new LoginRequest
        {
            Email = "",
            Senha = "SenhaForte123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = new LoginRequest
        {
            Email = "invalido",
            Senha = "SenhaForte123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email"
            && e.ErrorMessage.Contains("Email inválido"));
    }

    [Fact]
    public void Validate_WithEmptySenha_ShouldHaveError()
    {
        var request = new LoginRequest
        {
            Email = "joao@email.com",
            Senha = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }
}
