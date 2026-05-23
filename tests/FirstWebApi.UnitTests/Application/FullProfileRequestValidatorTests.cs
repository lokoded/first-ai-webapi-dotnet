using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class FullProfileRequestValidatorTests
{
    private readonly FullProfileRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidSenha_ShouldBeValid()
    {
        var request = new FullProfileRequest
        {
            Senha = "MinhaSenha123"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptySenha_ShouldHaveError()
    {
        var request = new FullProfileRequest
        {
            Senha = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }
}
