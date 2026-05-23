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
    private readonly Mock<IComicTypeRepository> _comicTypeRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ComicService _comicService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _comicTypeId = Guid.NewGuid();

    private Comic CreateComic(string titulo = "Batman", Guid? userId = null, Guid? comicTypeId = null) =>
        new(titulo, $"https://exemplo.com/{titulo.ToLower().Replace(' ', '-')}", userId ?? _userId, comicTypeId ?? _comicTypeId);

    public ComicServiceTests()
    {
        _comicRepoMock = new Mock<IComicRepository>();
        _comicTypeRepoMock = new Mock<IComicTypeRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _comicService = new ComicService(_comicRepoMock.Object, _unitOfWorkMock.Object, _comicTypeRepoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUserComics()
    {
        var comics = new List<Comic>
        {
            CreateComic(),
            CreateComic("Superman")
        };

        _comicRepoMock.Setup(r => r.GetPaginatedByUserIdAsync(_userId, 1, 20))
            .ReturnsAsync((comics, comics.Count));

        var result = await _comicService.GetAllAsync(_userId);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items[0].Titulo.Should().Be("Batman");
        result.Items[1].Titulo.Should().Be("Superman");
    }

    [Fact]
    public async Task GetByIdAsync_WithUserComic_ShouldReturnComic()
    {
        var comic = CreateComic();
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.GetByIdAsync(comic.Id, _userId);

        result.Should().NotBeNull();
        result!.Titulo.Should().Be("Batman");
    }

    [Fact]
    public async Task GetByIdAsync_WithOtherUserComic_ShouldReturnNull()
    {
        var outroUserId = Guid.NewGuid();
        var comic = CreateComic(userId: outroUserId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.GetByIdAsync(comic.Id, _userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddAndReturnComic()
    {
        var request = new ComicRequest
        {
            Titulo = "Batman",
            WebUrl = "https://exemplo.com/batman",
            ComicTypeId = _comicTypeId
        };

        _comicTypeRepoMock.Setup(r => r.GetByIdAsync(_comicTypeId))
            .ReturnsAsync(new ComicType("HQ"));

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
    public async Task UpdateAsync_WithUserComic_ShouldUpdateAndReturn()
    {
        var comic = CreateComic();
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
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithOtherUserComic_ShouldReturnNull()
    {
        var outroUserId = Guid.NewGuid();
        var comic = CreateComic(userId: outroUserId);
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
    public async Task DeleteAsync_WithUserComic_ShouldRemoveAndReturnTrue()
    {
        var comic = CreateComic();
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _comicService.DeleteAsync(comic.Id, _userId);

        result.Should().BeTrue();
        _comicRepoMock.Verify(r => r.DeleteAsync(comic), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithOtherUserComic_ShouldReturnFalse()
    {
        var outroUserId = Guid.NewGuid();
        var comic = CreateComic(userId: outroUserId);
        _comicRepoMock.Setup(r => r.GetByIdAsync(comic.Id)).ReturnsAsync(comic);

        var result = await _comicService.DeleteAsync(comic.Id, _userId);

        result.Should().BeFalse();
        _comicRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Comic>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentComic_ShouldReturnFalse()
    {
        _comicRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Comic?)null);

        var result = await _comicService.DeleteAsync(Guid.NewGuid(), _userId);

        result.Should().BeFalse();
    }
}
