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
    public async Task GetAllAsync_DeveRetornarTodosOsTipos()
    {
        var types = new List<ComicType>
        {
            new("Mangá"),
            new("HQ Americana"),
            new("Euro Quadrinhos")
        };

        _comicTypeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(types);

        var result = await _comicTypeService.GetAllAsync();

        result.Should().HaveCount(3);
        result[0].Nome.Should().Be("Mangá");
        result[1].Nome.Should().Be("HQ Americana");
        result[2].Nome.Should().Be("Euro Quadrinhos");
    }

    [Fact]
    public async Task CreateAsync_DeveAdicionarERetornarComicType()
    {
        _comicTypeRepoMock.Setup(r => r.AddAsync(It.IsAny<ComicType>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicTypeService.CreateAsync("Mangá");

        result.Should().NotBeNull();
        result.Nome.Should().Be("Mangá");
        _comicTypeRepoMock.Verify(r => r.AddAsync(It.IsAny<ComicType>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComTipoSemComics_DeveRemoverERetornarTrue()
    {
        var comicType = new ComicType("Mangá");
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(comicType.Id)).ReturnsAsync(comicType);
        _comicTypeRepoMock.Setup(r => r.HasComicsAsync(comicType.Id)).ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicTypeService.DeleteAsync(comicType.Id);

        result.Should().BeTrue();
        _comicTypeRepoMock.Verify(r => r.DeleteAsync(comicType), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComTipoComComics_DeveLancarExcecao()
    {
        var comicType = new ComicType("Mangá");
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(comicType.Id)).ReturnsAsync(comicType);
        _comicTypeRepoMock.Setup(r => r.HasComicsAsync(comicType.Id)).ReturnsAsync(true);

        Func<Task> act = () => _comicTypeService.DeleteAsync(comicType.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tipo possui comics vinculadas. Remova-as primeiro.");
    }

    [Fact]
    public async Task DeleteAsync_ComTipoInexistente_DeveRetornarFalse()
    {
        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ComicType?)null);

        var result = await _comicTypeService.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }
}
