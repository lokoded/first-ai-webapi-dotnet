using FirstWebApi.Application.Exceptions;
using FirstWebApi.Application.Services;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FirstWebApi.UnitTests.Application;

public class ComicTypeServiceTests
{
    private readonly Mock<IComicTypeRepository> _comicTypeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ComicTypeService _comicTypeService;

    public ComicTypeServiceTests()
    {
        _comicTypeRepoMock = new Mock<IComicTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _comicTypeService = new ComicTypeService(_comicTypeRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTypes()
    {
        var types = new List<ComicType>
        {
            new("Mangá"),
            new("HQ Americana"),
            new("Euro Quadrinhos")
        };

        _comicTypeRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(types);

        var result = await _comicTypeService.GetAllAsync();

        result.Should().HaveCount(3);
        result[0].Nome.Should().Be("Mangá");
        result[1].Nome.Should().Be("HQ Americana");
        result[2].Nome.Should().Be("Euro Quadrinhos");
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndReturnComicType()
    {
        _comicTypeRepoMock.Setup(r => r.AddAsync(It.IsAny<ComicType>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _comicTypeService.CreateAsync("Mangá");

        result.Should().NotBeNull();
        result.Nome.Should().Be("Mangá");
        _comicTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<ComicType>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithTypeWithoutComics_ShouldRemoveAndReturnTrue()
    {
        var comicType = new ComicType("Mangá");
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(comicType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comicType);
        _comicTypeRepoMock.Setup(r => r.HasComicsAsync(comicType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _comicTypeService.DeleteAsync(comicType.Id);

        result.Should().BeTrue();
        _comicTypeRepoMock.Verify(r => r.DeleteAsync(comicType, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithTypeWithComics_ShouldThrowException()
    {
        var comicType = new ComicType("Mangá");
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(comicType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(comicType);
        _comicTypeRepoMock.Setup(r => r.HasComicsAsync(comicType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => _comicTypeService.DeleteAsync(comicType.Id);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Tipo possui comics vinculadas. Remova-as primeiro.");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentType_ShouldReturnFalse()
    {
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ComicType?)null);

        var result = await _comicTypeService.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }
}
