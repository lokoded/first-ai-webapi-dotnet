using FirstWebApi.Application.DTOs.Request;
using FirstWebApi.Application.Services;
using FirstWebApi.Domain.Entities;
using FirstWebApi.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FirstWebApi.UnitTests.Application;

public class ComicServiceTests
{
    private readonly Mock<IComicRepository> _comicRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ComicService _comicService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _comicTypeId = Guid.NewGuid();

    public ComicServiceTests()
    {
        _comicRepoMock = new Mock<IComicRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _comicService = new ComicService(_comicRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_DeveRetornarComicsDoUsuario()
    {
        var comics = new List<Comic>
        {
            new("Batman", "https://exemplo.com/batman", _userId, _comicTypeId),
            new("Superman", "https://exemplo.com/superman", _userId, _comicTypeId)
        };

        _comicRepoMock.Setup(r => r.GetByUserIdAsync(_userId)).ReturnsAsync(comics);

        var result = await _comicService.GetAllAsync(_userId);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Titulo.Should().Be("Batman");
        result.Items[1].Titulo.Should().Be("Superman");
    }

    [Fact]
    public async Task GetByIdAsync_ComComicDoUsuario_DeveRetornarComic()
    {
        var comic = new Comic("Batman", "https://exemplo.com/batman", _userId, _comicTypeId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.GetByIdAsync(comic.Id, _userId);

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Batman");
    }

    [Fact]
    public async Task GetByIdAsync_ComComicDeOutroUsuario_DeveRetornarNull()
    {
        var outroUserId = Guid.NewGuid();
        var comic = new Comic("Batman", "https://exemplo.com/batman", outroUserId, _comicTypeId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.GetByIdAsync(comic.Id, _userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_DeveAdicionarERetornarComic()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "https://exemplo.com/batman",
            ComicTypeId = _comicTypeId
        };

        Comic? captured = null;
        _comicRepoMock.Setup(r => r.AddAsync(It.IsAny<Comic>()))
            .Callback<Comic>(c => captured = c)
            .Returns(Task.CompletedTask);

        _comicRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => captured);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicService.CreateAsync(request, _userId);

        result.Should().NotBeNull();
        result.Titulo.Should().Be("Batman");
        _comicRepoMock.Verify(r => r.AddAsync(It.IsAny<Comic>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComComicDoUsuario_DeveAtualizarERetornar()
    {
        var comic = new Comic("Batman", "https://exemplo.com/batman", _userId, _comicTypeId);
        var request = new ComicRequest
        {
            Titulo = "Batman 2024",
            WebUrl = "https://exemplo.com/batman-novo",
            ComicTypeId = _comicTypeId
        };

        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicService.UpdateAsync(comic.Id, request, _userId);

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Batman 2024");
        _comicRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Comic>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ComComicDeOutroUsuario_DeveRetornarNull()
    {
        var outroUserId = Guid.NewGuid();
        var comic = new Comic("Batman", "https://exemplo.com/batman", outroUserId, _comicTypeId);
        var request = new ComicRequest
        {
            Titulo = "Batman 2024",
            WebUrl = "https://exemplo.com/batman-novo",
            ComicTypeId = _comicTypeId
        };

        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.UpdateAsync(comic.Id, request, _userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ComComicDoUsuario_DeveRemoverERetornarTrue()
    {
        var comic = new Comic("Batman", "https://exemplo.com/batman", _userId, _comicTypeId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicService.DeleteAsync(comic.Id, _userId);

        result.Should().BeTrue();
        _comicRepoMock.Verify(r => r.DeleteAsync(comic), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ComComicDeOutroUsuario_DeveRetornarFalse()
    {
        var outroUserId = Guid.NewGuid();
        var comic = new Comic("Batman", "https://exemplo.com/batman", outroUserId, _comicTypeId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.DeleteAsync(comic.Id, _userId);

        result.Should().BeFalse();
        _comicRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Comic>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ComComicInexistente_DeveRetornarFalse()
    {
        _comicRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Comic?)null);

        var result = await _comicService.DeleteAsync(Guid.NewGuid(), _userId);

        result.Should().BeFalse();
    }
}
