using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class ComicTypeRequestValidatorTests
{
    private readonly ComicTypeRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidNome_ShouldBeValid()
    {
        var request = new ComicTypeRequest
        {
            Nome = "Mangá"
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyNome_ShouldHaveError()
    {
        var request = new ComicTypeRequest
        {
            Nome = ""
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }
}
