using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Validators;
using FluentAssertions;

namespace FirstWebApi.UnitTests.Application;

public class ComicRequestValidatorTests
{
    private readonly ComicRequestValidator _validator = new();

    [Fact]
    public void Validate_WithAllValidFields_ShouldBeValid()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "https://exemplo.com/batman",
            ComicTypeId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyTitulo_ShouldHaveError()
    {
        var request = new ComicRequest
        {
            Titulo = "",
            WebUrl = "https://exemplo.com/batman",
            ComicTypeId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Titulo");
    }

    [Fact]
    public void Validate_WithEmptyWebUrl_ShouldHaveError()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "",
            ComicTypeId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WebUrl");
    }

    [Fact]
    public void Validate_WithInvalidWebUrl_ShouldHaveError()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "not-a-url",
            ComicTypeId = Guid.NewGuid()
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WebUrl"
            && e.ErrorMessage.Contains("URL válida"));
    }

    [Fact]
    public void Validate_WithEmptyComicTypeId_ShouldHaveError()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "https://exemplo.com/batman",
            ComicTypeId = Guid.Empty
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ComicTypeId");
    }
}
